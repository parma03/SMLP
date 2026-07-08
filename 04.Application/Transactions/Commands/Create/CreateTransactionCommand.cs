using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Caching;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Caching;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Services.Email;
using Smlp.Application.Services.WhatsApp;
using Smlp.Application.Services.PushNotification;
using Smlp.Base.Customer.Enums;
using Smlp.Base.Transaction.Enums;
using Smlp.Domain.Entities.Transaction;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.Transactions.Commands.Create;

namespace Smlp.Application.Transactions.Commands.Create;

public class CreateTransactionCommand : IRequest<CommonCreateResponse>
{
    public CreateTransactionRequest Request { get; set; } = default!;
}

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(v => v.Request)
            .NotNull()
            .SetValidator(new CreateTransactionRequestValidator());
    }
}

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, CommonCreateResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IPushNotificationService _pushNotification;
    private readonly ICacheService _cache;

    public CreateTransactionCommandHandler(
        ISmlpDbContext context,
        IEmailService emailService,
        IWhatsAppService whatsAppService,
        IPushNotificationService pushNotification,
        ICacheService cache)
    {
        _context = context;
        _emailService = emailService;
        _whatsAppService = whatsAppService;
        _pushNotification = pushNotification;
        _cache = cache;
    }

    public async Task<CommonCreateResponse> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == req.CustomerId && !c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Customer", req.CustomerId);

        // Get active point rule
        var today = DateOnly.FromDateTime(DateTime.Today);
        var pointRule = await _context.PointRules
            .Where(r => r.IsActive && !r.IsDeleted
                && (r.ValidFrom == null || r.ValidFrom <= today)
                && (r.ValidUntil == null || r.ValidUntil >= today))
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var txCode = await GenerateTransactionCodeAsync(cancellationToken);
        var totalAmount = req.Items.Sum(i => i.UnitPrice * i.Quantity);
        var discountAmount = req.DiscountAmount;
        var grandTotal = Math.Max(0, totalAmount - discountAmount);

        var pointsEarned = 0;
        var multiplier = 1m;

        if (pointRule != null && pointRule.SpendAmount > 0)
        {
            multiplier = pointRule.BonusMultiplier ?? 1m;
            // Points dihitung dari grandTotal (setelah diskon)
            pointsEarned = (int)(grandTotal / pointRule.SpendAmount * pointRule.PointEarned * multiplier);
        }

        // Resolve payment method (backward-compat + multiple payment)
        var resolvedPaymentMethod = req.Payments.Any()
            ? req.Payments.OrderByDescending(p => p.Amount).First().PaymentMethod  // primary = largest payment
            : req.PaymentMethod;

        var transaction = new Transaksi
        {
            TransactionCode = txCode,
            CustomerId = customer.Id,
            TransactionDate = DateTimeOffset.UtcNow,
            TotalAmount = totalAmount,
            DiscountAmount = discountAmount,
            GrandTotal = grandTotal,
            PointsEarned = pointsEarned,
            BonusPoints = 0,
            PaymentMethod = resolvedPaymentMethod,
            Notes = req.Notes,
            Status = TransactionStatus.Completed,
            CustomerTierSnapshot = customer.Tier.ToString(),
            PointMultiplierSnapshot = multiplier,
            Items = req.Items.Select(i => new TransactionItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                SubTotal = i.UnitPrice * i.Quantity,
            }).ToList(),
        };

        // Build payments list
        if (req.Payments.Any())
        {
            foreach (var p in req.Payments)
            {
                transaction.Payments.Add(new TransactionPayment
                {
                    Id = Guid.NewGuid(),
                    PaymentMethod = p.PaymentMethod,
                    Amount = p.Amount,
                    Reference = p.Reference,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "system",
                });
            }
        }
        else
        {
            // Single payment (backward-compat)
            transaction.Payments.Add(new TransactionPayment
            {
                Id = Guid.NewGuid(),
                PaymentMethod = req.PaymentMethod,
                Amount = grandTotal,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "system",
            });
        }

        await _context.Transactions.AddAsync(transaction, cancellationToken);

        // Inventory stock deduction for items linked to a product
        var productIds = req.Items
            .Where(i => i.ProductId.HasValue)
            .Select(i => i.ProductId!.Value)
            .Distinct()
            .ToList();

        if (productIds.Any())
        {
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var item in req.Items.Where(i => i.ProductId.HasValue))
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId!.Value);
                if (product is not null && product.Stock > 0)
                {
                    product.Stock = Math.Max(0, product.Stock - item.Quantity);
                }
            }
        }

        // Update customer points and spend
        customer.TotalPoints += pointsEarned;
        customer.LifetimePoints += pointsEarned;
        customer.TotalSpend += grandTotal;
        customer.LastTransactionDate = transaction.TransactionDate;

        // Capture previous tier before upgrade check
        var previousTier = customer.Tier;

        // Tier upgrade automation: use TierRules from DB, fall back to hardcoded if none configured
        customer.Tier = await DetermineNewTierAsync(customer.LifetimePoints, cancellationToken);

        await _context.SaveChangesAsync(this, cancellationToken);

        // Transaksi baru mengubah angka-angka di dashboard (revenue, poin, jumlah transaksi) — invalidate cache-nya
        await _cache.RemoveAsync(CacheKeys.DashboardSummary, cancellationToken);

        // Push notification real-time ke Admin & Kasir (fire-and-forget)
        _ = SendPushNotificationsAsync(customer.FullName, transaction.TransactionCode, grandTotal, pointsEarned);

        // Fire-and-forget: kirim notifikasi email & WhatsApp tanpa block response
        _ = SendNotificationsAsync(customer, transaction.TransactionCode, grandTotal, pointsEarned, previousTier, customer.Tier);

        return new CommonCreateResponse { Id = transaction.Id };
    }

    /// <summary>
    /// Kirim push notification real-time via SignalR ke role Admin dan Kasir.
    /// Error di-swallow agar tidak menggagalkan transaksi utama.
    /// </summary>
    private async Task SendPushNotificationsAsync(
        string customerName,
        string transactionCode,
        decimal grandTotal,
        int pointsEarned)
    {
        try
        {
            var title = "Transaksi Baru";
            var message = $"{customerName} — {transactionCode} | Rp {grandTotal:N0} | +{pointsEarned} poin";
            var actionUrl = $"/Transactions";

            await _pushNotification.SendToRoleAsync("Admin",  title, message, "success", actionUrl);
            await _pushNotification.SendToRoleAsync("Kasir",  title, message, "success", actionUrl);
        }
        catch { /* swallow — push notification bukan critical path */ }
    }

    /// <summary>
    /// Kirim notifikasi transaksi + tier upgrade via email &amp; WhatsApp (fire-and-forget).
    /// Error tidak di-throw agar tidak menggagalkan transaksi utama.
    /// </summary>
    private async Task SendNotificationsAsync(
        Domain.Entities.Master.Customer customer,
        string transactionCode,
        decimal grandTotal,
        int pointsEarned,
        CustomerTier previousTier,
        CustomerTier newTier)
    {
        try
        {
            var hasTierUpgrade = newTier != previousTier;
            var email = customer.EmailAddress;
            var phone = customer.PhoneNumber;
            var name = customer.FullName;

            // Email: konfirmasi transaksi
            if (!string.IsNullOrWhiteSpace(email))
            {
                try
                {
                    await _emailService.SendTransactionConfirmationAsync(
                        email, name, transactionCode, grandTotal, pointsEarned);
                }
                catch { /* swallow — notifikasi bukan critical path */ }

                // Email: tier upgrade
                if (hasTierUpgrade)
                {
                    try
                    {
                        await _emailService.SendTierUpgradeAsync(
                            email, name, previousTier.ToString(), newTier.ToString(), customer.TotalPoints);
                    }
                    catch { /* swallow */ }
                }
            }

            // WhatsApp: konfirmasi transaksi
            if (!string.IsNullOrWhiteSpace(phone))
            {
                try
                {
                    await _whatsAppService.SendTransactionConfirmationAsync(
                        phone, name, transactionCode, grandTotal, pointsEarned);
                }
                catch { /* swallow */ }

                // WhatsApp: tier upgrade
                if (hasTierUpgrade)
                {
                    try
                    {
                        await _whatsAppService.SendTierUpgradeAsync(
                            phone, name, newTier.ToString(), customer.TotalPoints);
                    }
                    catch { /* swallow */ }
                }
            }
        }
        catch { /* outer safety net */ }
    }

    /// <summary>
    /// Determines the customer tier based on lifetime points using configured TierRules.
    /// Falls back to hardcoded defaults if no TierRules are configured.
    /// </summary>
    private async Task<CustomerTier> DetermineNewTierAsync(int lifetimePoints, CancellationToken ct)
    {
        var tierRules = await _context.TierRules
            .Where(r => r.IsActive && !r.IsDeleted)
            .OrderByDescending(r => r.MinimumLifetimePoints)
            .ToListAsync(ct);

        if (tierRules.Any())
        {
            foreach (var rule in tierRules)
            {
                var maxOk = rule.MaximumLifetimePoints == 0 || lifetimePoints <= rule.MaximumLifetimePoints;
                if (lifetimePoints >= rule.MinimumLifetimePoints && maxOk)
                {
                    return rule.Tier;
                }
            }
            return tierRules.OrderBy(r => r.MinimumLifetimePoints).First().Tier;
        }

        // Fallback to hardcoded defaults
        return lifetimePoints switch
        {
            >= 15000 => CustomerTier.Platinum,
            >= 5000 => CustomerTier.Gold,
            >= 1000 => CustomerTier.Silver,
            _ => CustomerTier.Bronze,
        };
    }

    private async Task<string> GenerateTransactionCodeAsync(CancellationToken ct)
    {
        var count = await _context.Transactions.CountAsync(ct);
        var date = DateTime.Today.ToString("yyyyMMdd");
        return $"TRX-{date}-{count + 1:D5}";
    }
}
