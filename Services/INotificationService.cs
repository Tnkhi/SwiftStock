using SwiftStock.Models;

namespace SwiftStock.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(NotificationType type, NotificationPriority priority, string title, 
                                   string message, string? userId = null, string? actionUrl = null, 
                                   string? actionText = null, string? createdById = null);
        
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, 
                                                                 int page = 1, int pageSize = 20);
        
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        
        Task CreateLowStockNotificationAsync(int productId, int currentStock, int minStock);
        Task CreateSystemNotificationAsync(string title, string message, NotificationPriority priority = NotificationPriority.Medium);
    }
}

