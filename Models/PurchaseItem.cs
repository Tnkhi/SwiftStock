using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }

        [Required]
        public int PurchaseId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public int? ReceivedQuantity { get; set; } // Quantité réellement reçue

        // Navigation properties
        public virtual Purchase Purchase { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}

