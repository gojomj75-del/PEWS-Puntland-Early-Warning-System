using PEWS.Models;
using PEWS.Models.Enums;

namespace PEWS.Services;

public interface INotificationService
{
    Task CreateAsync(string userId, string title, string message, NotificationType type, int? relatedEntityId = null, string? relatedEntityType = null, string? actionUrl = null);
    Task BroadcastAsync(string title, string message, NotificationType type, int? relatedEntityId = null, string? relatedEntityType = null);
    Task MarkReadAsync(int notificationId, string userId);
    Task MarkAllReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task<IEnumerable<Notification>> GetRecentAsync(string userId, int count = 10);
}
