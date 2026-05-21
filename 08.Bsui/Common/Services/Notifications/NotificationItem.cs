namespace Smlp.Bsui.Common.Services.Notifications;

public class NotificationItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationLevel Level { get; set; } = NotificationLevel.Info;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    public bool IsRead { get; set; } = false;
    public string? ActionUrl { get; set; }
    public string? ActionLabel { get; set; }

    public string Icon => Level switch
    {
        NotificationLevel.Success => Icons.Material.Filled.CheckCircle,
        NotificationLevel.Warning => Icons.Material.Filled.Warning,
        NotificationLevel.Error => Icons.Material.Filled.Error,
        _ => Icons.Material.Filled.Notifications
    };

    public string Color => Level switch
    {
        NotificationLevel.Success => "#2E7D32",
        NotificationLevel.Warning => "#F57F17",
        NotificationLevel.Error => "#C62828",
        _ => "#1565C0"
    };
}

public enum NotificationLevel
{
    Info,
    Success,
    Warning,
    Error
}

// Blazor Icons reference (same as MudBlazor)
static file class Icons
{
    public static class Material
    {
        public static class Filled
        {
            public const string CheckCircle = "@Icons.Material.Filled.CheckCircle";
            public const string Warning = "@Icons.Material.Filled.Warning";
            public const string Error = "@Icons.Material.Filled.Error";
            public const string Notifications = "@Icons.Material.Filled.Notifications";
        }
    }
}
