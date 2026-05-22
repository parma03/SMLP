namespace Smlp.Bsui.Common.Services.Notifications;

/// <summary>
/// In-app notification service. Inject as Scoped so state is per-user-session.
/// </summary>
public class NotificationService
{
    private readonly List<NotificationItem> _items = new();

    public IReadOnlyList<NotificationItem> Items => _items.AsReadOnly();
    public int UnreadCount => _items.Count(i => !i.IsRead);
    public bool HasUnread => UnreadCount > 0;

    public event Action? OnChange;

    public void Add(string title, string message, NotificationLevel level = NotificationLevel.Info, string? actionUrl = null, string? actionLabel = null)
    {
        _items.Insert(0, new NotificationItem
        {
            Title = title,
            Message = message,
            Level = level,
            ActionUrl = actionUrl,
            ActionLabel = actionLabel,
        });
        // Keep max 50 notifications
        if (_items.Count > 50)
            _items.RemoveRange(50, _items.Count - 50);
        NotifyStateChanged();
    }

    public void MarkRead(Guid id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item is not null)
        { item.IsRead = true; NotifyStateChanged(); }
    }

    public void MarkAllRead()
    {
        _items.ForEach(i => i.IsRead = true);
        NotifyStateChanged();
    }

    public void Remove(Guid id)
    {
        _items.RemoveAll(i => i.Id == id);
        NotifyStateChanged();
    }

    public void Clear()
    {
        _items.Clear();
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
