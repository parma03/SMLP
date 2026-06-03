namespace Smlp.Bsui.Services.PushNotification;

/// <summary>
/// Konfigurasi untuk PushHubService — dibaca dari appsettings.json bagian "BackEnd".
/// </summary>
public class PushHubOptions
{
    public const string SectionKey = "BackEnd";

    /// <summary>
    /// URL dasar WebApi, misal https://localhost:7001.
    /// Hub endpoint akan di-resolve menjadi {ApiBaseUrl}/hubs/notification.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://localhost:7001";
}
