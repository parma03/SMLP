namespace Smlp.Application.Services.PushNotification;

/// <summary>
/// Abstraksi push notification untuk Application layer.
/// Implementasi dilakukan di WebApi (SignalR) atau None (no-op).
/// </summary>
public interface IPushNotificationService
{
    Task BroadcastAsync(string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default);
    Task SendToUserAsync(string userId, string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default);
    Task SendToRoleAsync(string role, string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default);
    Task SendToGroupAsync(string group, string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default);
}
