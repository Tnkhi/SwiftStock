using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public enum CountStatus
    {
        NotCounted,     // Non compté
        Counted,        // Compté
        Discrepancy,    // Écart
        Verified        // Vérifié
    }

    public class PhysicalInventoryItem
    {
        public int Id { get; set; }

        [Required]
        public int PhysicalInventoryId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

        public int SystemStock { get; set; } // Stock selon le système
        public int? CountedStock { get; set; } // Stock compté
        public int? Discrepancy { get; set; } // Écart (CountedStock - SystemStock)

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal DiscrepancyValue { get; set; } = 0;

        [Required]
        public CountStatus Status { get; set; } = CountStatus.NotCounted;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? CountedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }

        [StringLength(450)]
        public string? CountedById { get; set; }

        [StringLength(450)]
        public string? VerifiedById { get; set; }

        // Navigation properties
        public virtual PhysicalInventory PhysicalInventory { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ProductVariant? ProductVariant { get; set; }
        public virtual User? CountedBy { get; set; }
        public virtual User? VerifiedBy { get; set; }
    }
}

