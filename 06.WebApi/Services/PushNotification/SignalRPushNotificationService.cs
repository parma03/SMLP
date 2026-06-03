using Microsoft.AspNetCore.SignalR;
using Smlp.Application.Services.PushNotification;
using Smlp.WebApi.Hubs;

namespace Smlp.WebApi.Services.PushNotification;

public class SignalRPushNotificationService : IPushNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRPushNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task BroadcastAsync(string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default)
        => _hubContext.Clients.All.SendAsync("ReceivePushNotification", Build(title, message, level, actionUrl), ct);

    public Task SendToUserAsync(string userId, string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default)
        => _hubContext.Clients.User(userId).SendAsync("ReceivePushNotification", Build(title, message, level, actionUrl), ct);

    public Task SendToRoleAsync(string role, string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default)
        => _hubContext.Clients.Group($"role_{role}").SendAsync("ReceivePushNotification", Build(title, message, level, actionUrl), ct);

    public Task SendToGroupAsync(string group, string title, string message, string level = "info", string? actionUrl = null, CancellationToken ct = default)
        => _hubContext.Clients.Group(group).SendAsync("ReceivePushNotification", Build(title, message, level, actionUrl), ct);

    private static object Build(string title, string message, string level, string? actionUrl)
        => new
        {
            id = Guid.NewGuid().ToString(),
            title,
            message,
            level,
            actionUrl,
            createdAt = DateTimeOffset.UtcNow
        };
}
