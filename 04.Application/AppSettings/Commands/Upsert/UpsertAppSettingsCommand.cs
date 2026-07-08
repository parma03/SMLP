using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.AppSettings.Commands.UpsertAppSettings;

namespace Smlp.Application.AppSettings.Commands.Upsert;

public class UpsertAppSettingsCommand : IRequest
{
    public UpsertAppSettingsRequest Request { get; set; } = default!;
}

public class UpsertAppSettingsCommandHandler : IRequestHandler<UpsertAppSettingsCommand>
{
    private readonly ISmlpDbContext _context;

    public UpsertAppSettingsCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<Unit> Handle(UpsertAppSettingsCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        var settingsToUpsert = new Dictionary<string, (string? value, string label, string group, string description)>
        {
            ["Shop.Name"] = (req.ShopName, "Nama Coffee Shop", "Shop", "Nama resmi coffee shop"),
            ["Shop.Address"] = (req.ShopAddress, "Alamat", "Shop", "Alamat lengkap coffee shop"),
            ["Shop.Phone"] = (req.ShopPhone, "No. Telepon", "Shop", "Nomor telepon coffee shop"),
            ["Shop.Email"] = (req.ShopEmail, "Email", "Shop", "Email coffee shop"),
            ["Shop.LogoUrl"] = (req.ShopLogoUrl, "Logo URL", "Shop", "URL logo coffee shop"),
            ["Loyalty.PointExpireDays"] = (req.LoyaltyPointExpireDays?.ToString(), "Masa Berlaku Poin (hari)", "Loyalty", "Jumlah hari sebelum poin kadaluarsa (0 = tidak kadaluarsa)"),
            ["Loyalty.MinRedeemPoints"] = (req.LoyaltyMinRedeemPoints?.ToString(), "Minimum Poin Redeem", "Loyalty", "Minimum poin yang harus dimiliki untuk redeem reward"),
            ["Loyalty.WelcomeBonus"] = (req.LoyaltyWelcomeBonus?.ToString(), "Bonus Selamat Datang", "Loyalty", "Poin bonus diberikan saat pelanggan baru didaftarkan"),
            ["Receipt.FooterMessage"] = (req.ReceiptFooterMessage, "Pesan Footer Struk", "Receipt", "Teks yang tampil di bagian bawah struk transaksi"),
        };

        var existingKeys = await _context.AppSettings
            .Where(s => settingsToUpsert.Keys.Contains(s.Key) && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var (key, (value, label, group, description)) in settingsToUpsert)
        {
            var existing = existingKeys.FirstOrDefault(s => s.Key == key);
            if (existing is not null)
            {
                existing.Value = value;
                existing.Label = label;
            }
            else
            {
                await _context.AppSettings.AddAsync(new AppSetting
                {
                    Key = key,
                    Value = value,
                    Label = label,
                    Group = group,
                    Description = description,
                }, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(this, cancellationToken);

        return Unit.Value;
    }
}
