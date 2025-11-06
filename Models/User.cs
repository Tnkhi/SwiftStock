using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SwiftStock.Models
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Role { get; set; } // Admin, Manager, Cashier

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public DateTime? LastPasswordChange { get; set; }
        public bool IsActive { get; set; } = true;
        public bool MustChangePassword { get; set; } = false;

        // Relations avec les nouveaux modèles
        public int? UserRoleId { get; set; }
        public int? DefaultLocationId { get; set; }

        // Navigation properties
        public virtual UserRole? UserRole { get; set; }
        public virtual Location? DefaultLocation { get; set; }
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public virtual ICollection<Location> CreatedLocations { get; set; } = new List<Location>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Notification> CreatedNotifications { get; set; } = new List<Notification>();
    }
}

