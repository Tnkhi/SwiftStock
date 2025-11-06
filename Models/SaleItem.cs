using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class SaleItem
    {
        public int Id { get; set; }

        [Required]
        public int SaleId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

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

        // Navigation properties
        public virtual Sale Sale { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ProductVariant? ProductVariant { get; set; }
    }
}

