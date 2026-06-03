namespace Smlp.Bsui.Services.PushNotification;

/// <summary>
/// Payload yang diterima dari SignalR event "ReceivePushNotification".
/// Harus cocok dengan payload yang dikirim oleh SignalRPushNotificationService di WebApi.
/// </summary>
public record PushNotificationPayload(
    string Id,
    string Title,
    string Message,
    string Level,       // "info" | "success" | "warning" | "error"
    string? ActionUrl,
    DateTimeOffset CreatedAt
);
