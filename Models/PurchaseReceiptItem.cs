using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class PurchaseReceiptItem
    {
        public int Id { get; set; }

        [Required]
        public int PurchaseReceiptId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

        [Required]
        public int ReceivedQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(100)]
        public string? BatchNumber { get; set; }

        // Navigation properties
        public virtual PurchaseReceipt PurchaseReceipt { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ProductVariant? ProductVariant { get; set; }
    }
}

