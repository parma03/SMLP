using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.AppSettings.Queries.GetAppSettings;

namespace Smlp.Application.AppSettings.Queries;

public class GetAppSettingsQuery : IRequest<GetAppSettingsResponse> { }

public class GetAppSettingsQueryHandler : IRequestHandler<GetAppSettingsQuery, GetAppSettingsResponse>
{
    private readonly ISmlpDbContext _context;

    public GetAppSettingsQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<GetAppSettingsResponse> Handle(GetAppSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _context.AppSettings
            .Where(s => !s.IsDeleted)
            .ToListAsync(cancellationToken);

        var dict = settings.ToDictionary(s => s.Key, s => s.Value);

        return new GetAppSettingsResponse
        {
            ShopName = dict.GetValueOrDefault("Shop.Name") ?? "SMLP Coffee Shop",
            ShopAddress = dict.GetValueOrDefault("Shop.Address") ?? string.Empty,
            ShopPhone = dict.GetValueOrDefault("Shop.Phone") ?? string.Empty,
            ShopEmail = dict.GetValueOrDefault("Shop.Email") ?? string.Empty,
            ShopLogoUrl = dict.GetValueOrDefault("Shop.LogoUrl"),
            LoyaltyPointExpireDays = int.TryParse(dict.GetValueOrDefault("Loyalty.PointExpireDays"), out var days) ? days : 365,
            LoyaltyMinRedeemPoints = int.TryParse(dict.GetValueOrDefault("Loyalty.MinRedeemPoints"), out var minPts) ? minPts : 100,
            LoyaltyWelcomeBonus = int.TryParse(dict.GetValueOrDefault("Loyalty.WelcomeBonus"), out var wb) ? wb : 0,
            ReceiptFooterMessage = dict.GetValueOrDefault("Receipt.FooterMessage") ?? "Terima kasih telah berbelanja!",
        };
    }
}
