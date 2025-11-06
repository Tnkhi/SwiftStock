using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public enum PurchaseOrderStatus
    {
        Draft,          // Brouillon
        Sent,           // Envoyé
        Confirmed,      // Confirmé
        PartiallyReceived, // Partiellement reçu
        Received,       // Reçu
        Cancelled,      // Annulé
        Closed          // Fermé
    }

    public class PurchaseOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public int SupplierId { get; set; }

        [Required]
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ReceivedAmount { get; set; } = 0;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(500)]
        public string? Terms { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string CreatedById { get; set; } = string.Empty;

        [StringLength(450)]
        public string? ApprovedById { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // Navigation properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual User CreatedBy { get; set; } = null!;
        public virtual User? ApprovedBy { get; set; }
        public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
        public virtual ICollection<PurchaseReceipt> Receipts { get; set; } = new List<PurchaseReceipt>();
    }
}

