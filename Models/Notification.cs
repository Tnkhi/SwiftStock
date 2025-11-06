using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public enum NotificationType
    {
        LowStock,
        ExpiredProduct,
        OverduePayment,
        SystemAlert,
        UserAction,
        BackupComplete,
        Error
    }

    public enum NotificationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public NotificationPriority Priority { get; set; } = NotificationPriority.Medium;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? ActionUrl { get; set; }

        [StringLength(50)]
        public string? ActionText { get; set; }

        public bool IsRead { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        [StringLength(450)]
        public string? CreatedById { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual User? CreatedBy { get; set; }
    }
}

