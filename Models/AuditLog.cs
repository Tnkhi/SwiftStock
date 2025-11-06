using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public enum AuditAction
    {
        Create,
        Update,
        Delete,
        Login,
        Logout,
        View,
        Export,
        Import,
        Void,
        Approve,
        Reject
    }

    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public AuditAction Action { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty;

        [StringLength(50)]
        public string? EntityId { get; set; }

        [StringLength(200)]
        public string? EntityName { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? OldValues { get; set; }

        [StringLength(1000)]
        public string? NewValues { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? UserId { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
    }
}

