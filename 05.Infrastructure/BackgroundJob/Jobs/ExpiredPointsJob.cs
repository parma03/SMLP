/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Expired Points Background Job Implementation
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Smlp.Application.BackgroundJobs;
using Smlp.Application.Services.Persistence;
using Smlp.Base.Customer.Enums;
using Smlp.Base.Transaction.Enums;
using Smlp.Domain.Entities.Transaction;

namespace Smlp.Infrastructure.BackgroundJob.Jobs;

/// <summary>
/// Implementasi job untuk mengkadaluarsakan poin pelanggan yang sudah melewati masa berlaku.
/// </summary>
public class ExpiredPointsJob : IExpiredPointsJob
{
    private readonly ISmlpDbContext _context;
    private readonly ILogger<ExpiredPointsJob> _logger;

    public ExpiredPointsJob(ISmlpDbContext context, ILogger<ExpiredPointsJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting expired points job at {Time}", DateTimeOffset.UtcNow);

        try
        {
            // Ambil konfigurasi expiry days dari AppSettings
            var expirySetting = await _context.AppSettings
                .FirstOrDefaultAsync(s => s.Key == "Loyalty.PointExpireDays" && !s.IsDeleted);

            if (!int.TryParse(expirySetting?.Value, out var expireDays) || expireDays <= 0)
            {
                _logger.LogInformation("Point expiry disabled (LoyaltyPointExpireDays <= 0). Skipping.");
                return;
            }

            var cutoffDate = DateTimeOffset.UtcNow.AddDays(-expireDays);

            // Cari pelanggan yang memiliki poin dan last transaction sebelum cutoff
            // atau tidak pernah transaksi setelah cutoff
            var customersToExpire = await _context.Customers
                .Where(c => !c.IsDeleted
                    && c.TotalPoints > 0
                    && (c.LastTransactionDate == null || c.LastTransactionDate <= cutoffDate))
                .ToListAsync();

            if (!customersToExpire.Any())
            {
                _logger.LogInformation("No customers with expired points found.");
                return;
            }

            _logger.LogInformation("Found {Count} customers with potentially expired points.", customersToExpire.Count);

            var now = DateTimeOffset.UtcNow;
            var adjustments = new List<PointAdjustment>();

            foreach (var customer in customersToExpire)
            {
                var expiredPoints = customer.TotalPoints;

                _logger.LogInformation(
                    "Expiring {Points} points for customer {CustomerId} ({Name}). Last transaction: {LastTx}",
                    expiredPoints, customer.Id, customer.FullName, customer.LastTransactionDate);

                // Buat adjustment record untuk audit trail
                adjustments.Add(new PointAdjustment
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    Points = -expiredPoints,
                    AdjustmentType = AdjustmentType.Expiry,
                    Reason = $"Poin kadaluarsa otomatis setelah {expireDays} hari tidak ada transaksi. " +
                             $"(Cutoff: {cutoffDate:dd/MM/yyyy}, Last Tx: {customer.LastTransactionDate?.ToString("dd/MM/yyyy") ?? "Tidak ada"})",
                    ApprovedBy = "System",
                    AdjustmentDate = now,
                    CreatedAt = now,
                    CreatedBy = "System.ExpiredPointsJob",
                });

                // Reset poin ke 0 (poin kadaluarsa tidak mengurangi LifetimePoints)
                customer.TotalPoints = 0;

                // Recalculate tier (jika poin habis, tier mungkin turun)
                // Tier berdasarkan LifetimePoints, jadi tidak berubah saat expiry
                // Hanya TotalPoints (current/active) yang diset 0
            }

            if (adjustments.Any())
            {
                await _context.PointAdjustments.AddRangeAsync(adjustments);
                // SaveChanges manual karena job tidak memiliki THandler context
                await ((Microsoft.EntityFrameworkCore.DbContext)_context).SaveChangesAsync();

                _logger.LogInformation(
                    "Expired points job completed. Processed {Count} customers, total points expired.",
                    customersToExpire.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in expired points job");
            throw;
        }
    }
}
