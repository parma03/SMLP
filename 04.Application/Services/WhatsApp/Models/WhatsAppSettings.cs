namespace Smlp.Application.Services.WhatsApp.Models;

public class WhatsAppSettings
{
    public string Provider { get; set; } = "None"; // None, Fonnte, Twilio
    public FonnteSettings? Fonnte { get; set; }
}

public class FonnteSettings
{
    /// <summary>API token from Fonnte dashboard (https://fonnte.com)</summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>Sender device (leave empty to use default device)</summary>
    public string? DeviceId { get; set; }
}
