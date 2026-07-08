using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.AppSettings.Queries.GetAppSettings;

public class GetAppSettingsResponse : Response
{
    // Shop info
    public string ShopName { get; set; } = "SMLP Coffee Shop";
    public string ShopAddress { get; set; } = string.Empty;
    public string ShopPhone { get; set; } = string.Empty;
    public string ShopEmail { get; set; } = string.Empty;
    public string? ShopLogoUrl { get; set; }

    // Loyalty config
    public int LoyaltyPointExpireDays { get; set; } = 365;
    public int LoyaltyMinRedeemPoints { get; set; } = 100;
    public int LoyaltyWelcomeBonus { get; set; } = 0;

    // Receipt
    public string ReceiptFooterMessage { get; set; } = "Terima kasih telah berbelanja!";
}
