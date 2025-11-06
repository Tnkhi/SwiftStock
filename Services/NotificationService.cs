using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateNotificationAsync(NotificationType type, NotificationPriority priority, string title, 
                                               string message, string? userId = null, string? actionUrl = null, 
                                               string? actionText = null, string? createdById = null)
        {
            try
            {
                var notification = new Notification
                {
                    Type = type,
                    Priority = priority,
                    Title = title,
                    Message = message,
                    UserId = userId,
                    ActionUrl = actionUrl,
                    ActionText = actionText,
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la notification: {Title}", title);
            }
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, 
                                                                             int page = 1, int pageSize = 20)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId && !n.IsArchived)
                .AsQueryable();

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsArchived);
        }

        public async Task CreateLowStockNotificationAsync(int productId, int currentStock, int minStock)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return;

            var title = "Stock bas détecté";
            var message = $"Le produit '{product.Name}' a un stock de {currentStock} unités (minimum: {minStock})";

            await CreateNotificationAsync(
                NotificationType.LowStock,
                NotificationPriority.High,
                title,
                message,
                actionUrl: $"/Products/Details/{productId}",
                actionText: "Voir le produit"
            );
        }

        public async Task CreateSystemNotificationAsync(string title, string message, NotificationPriority priority = NotificationPriority.Medium)
        {
            // Créer une notification pour tous les utilisateurs actifs
            var activeUsers = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var userId in activeUsers)
            {
                await CreateNotificationAsync(
                    NotificationType.SystemAlert,
                    priority,
                    title,
                    message,
                    userId
                );
            }
        }
    }
}

