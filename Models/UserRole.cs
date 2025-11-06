using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class UserRole
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        // Permissions par module
        public bool CanManageUsers { get; set; } = false;
        public bool CanManageProducts { get; set; } = false;
        public bool CanManageStock { get; set; } = false;
        public bool CanProcessSales { get; set; } = false;
        public bool CanManagePurchases { get; set; } = false;
        public bool CanViewReports { get; set; } = false;
        public bool CanManageSettings { get; set; } = false;

        // Permissions spécifiques
        public bool CanVoidSales { get; set; } = false;
        public bool CanApplyDiscounts { get; set; } = false;
        public bool CanManageInventory { get; set; } = false;
        public bool CanExportData { get; set; } = false;

        public bool IsActive { get; set; } = true;
        public bool IsSystemRole { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? CreatedById { get; set; }

        // Navigation properties
        public virtual User? CreatedBy { get; set; }
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}

