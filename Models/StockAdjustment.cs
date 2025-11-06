using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public enum AdjustmentType
    {
        Manual,         // Ajustement manuel
        PhysicalCount,  // Inventaire physique
        Damage,         // Casse
        Theft,          // Vol
        Expired,        // Périmé
        Return,         // Retour
        Transfer,       // Transfert
        Other           // Autre
    }

    public enum AdjustmentStatus
    {
        Pending,        // En attente
        Approved,       // Approuvé
        Rejected        // Rejeté
    }

    public class StockAdjustment
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

        [Required]
        public AdjustmentType Type { get; set; }

        [Required]
        public AdjustmentStatus Status { get; set; } = AdjustmentStatus.Pending;

        [Required]
        public int QuantityDifference { get; set; } // Positif = ajout, Négatif = retrait

        public int PreviousStock { get; set; }
        public int NewStock { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; } // Numéro de référence

        public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }

        [StringLength(450)]
        public string CreatedById { get; set; } = string.Empty;

        [StringLength(450)]
        public string? ApprovedById { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual ProductVariant? ProductVariant { get; set; }
        public virtual User CreatedBy { get; set; } = null!;
        public virtual User? ApprovedBy { get; set; }
    }
}

