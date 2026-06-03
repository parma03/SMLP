namespace Smlp.Application.Services.PushNotification;

/// <summary>
/// No-op implementation (digunakan ketika SignalR tidak dikonfigurasi / unit tests).
/// </summary>
public class NoOpPushNotificationService : IPushNotificationService
{
    public Task BroadcastAsync(string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default) => Task.CompletedTask;
    public Task SendToUserAsync(string userId, string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default) => Task.CompletedTask;
    public Task SendToRoleAsync(string role, string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default) => Task.CompletedTask;
    public Task SendToGroupAsync(string group, string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default) => Task.CompletedTask;
}
