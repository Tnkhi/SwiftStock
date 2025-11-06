using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public enum InventoryStatus
    {
        Planned,        // Planifié
        InProgress,     // En cours
        Completed,      // Terminé
        Cancelled       // Annulé
    }

    public class PhysicalInventory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public InventoryStatus Status { get; set; } = InventoryStatus.Planned;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? PlannedDate { get; set; }

        [StringLength(100)]
        public string? Location { get; set; } // Zone ou emplacement spécifique

        public bool IncludeInactiveProducts { get; set; } = false;
        public bool AutoAdjustStock { get; set; } = true;

        public int TotalProducts { get; set; } = 0;
        public int CountedProducts { get; set; } = 0;
        public int Discrepancies { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal TotalValue { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal DiscrepancyValue { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string CreatedById { get; set; } = string.Empty;

        // Navigation properties
        public virtual User CreatedBy { get; set; } = null!;
        public virtual ICollection<PhysicalInventoryItem> Items { get; set; } = new List<PhysicalInventoryItem>();
    }
}

