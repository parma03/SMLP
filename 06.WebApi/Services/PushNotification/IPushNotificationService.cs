namespace Smlp.WebApi.Services.PushNotification;

/// <summary>
/// Service untuk mengirim push notification ke Blazor client via SignalR.
/// </summary>
public interface IPushNotificationService
{
    /// <summary>Kirim ke semua user yang terkoneksi.</summary>
    Task BroadcastAsync(string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default);

    /// <summary>Kirim ke user spesifik berdasarkan userId.</summary>
    Task SendToUserAsync(string userId, string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default);

    /// <summary>Kirim ke semua user dengan role tertentu (misal: "Admin", "Kasir").</summary>
    Task SendToRoleAsync(string role, string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default);

    /// <summary>Kirim ke group custom.</summary>
    Task SendToGroupAsync(string group, string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default);
}
