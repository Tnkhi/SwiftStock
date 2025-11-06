using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class PurchaseOrderItem
    {
        public int Id { get; set; }

        [Required]
        public int PurchaseOrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

        [Required]
        public int OrderedQuantity { get; set; }

        public int ReceivedQuantity { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxRate { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ProductVariant? ProductVariant { get; set; }
    }
}

