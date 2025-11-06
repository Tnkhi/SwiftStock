using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public enum MovementType
    {
        Purchase,    // Achat
        Sale,        // Vente
        Adjustment,  // Ajustement manuel
        Return,      // Retour
        Transfer,    // Transfert entre emplacements
        Loss         // Perte/Casse
    }

    public class StockMovement
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

        [Required]
        public MovementType Type { get; set; }

        [Required]
        public int Quantity { get; set; } // Positif pour entrée, négatif pour sortie

        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitPrice { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(50)]
        public string? Reference { get; set; } // Référence de la vente, achat, etc.

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? CreatedById { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual ProductVariant? ProductVariant { get; set; }
        public virtual User? CreatedBy { get; set; }
    }
}

