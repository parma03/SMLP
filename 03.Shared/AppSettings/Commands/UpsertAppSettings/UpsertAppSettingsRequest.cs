namespace Smlp.Shared.AppSettings.Commands.UpsertAppSettings;

public class UpsertAppSettingsRequest
{
    // Shop info
    public string? ShopName { get; set; }
    public string? ShopAddress { get; set; }
    public string? ShopPhone { get; set; }
    public string? ShopEmail { get; set; }
    public string? ShopLogoUrl { get; set; }

    // Loyalty config
    public int? LoyaltyPointExpireDays { get; set; }
    public int? LoyaltyMinRedeemPoints { get; set; }
    public int? LoyaltyWelcomeBonus { get; set; }

    // Receipt
    public string? ReceiptFooterMessage { get; set; }
}
