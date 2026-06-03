namespace Smlp.WebApi.Services.PushNotification;

/// <summary>
/// Payload yang dikirim via SignalR ke Blazor client.
/// </summary>
public record PushNotificationPayload(
    string Id,
    string Title,
    string Message,
    string Level,       // "info" | "success" | "warning" | "error"
    string? ActionUrl,
    DateTimeOffset CreatedAt
);
