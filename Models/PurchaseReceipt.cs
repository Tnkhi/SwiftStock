using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public enum ReceiptStatus
    {
        Draft,          // Brouillon
        Partial,        // Partiel
        Complete,       // Complet
        Verified,       // Vérifié
        Cancelled       // Annulé
    }

    public class PurchaseReceipt
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ReceiptNumber { get; set; } = string.Empty;

        [Required]
        public int PurchaseOrderId { get; set; }

        [Required]
        public ReceiptStatus Status { get; set; } = ReceiptStatus.Draft;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? DeliveryReference { get; set; }

        [StringLength(100)]
        public string? InvoiceNumber { get; set; }

        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string CreatedById { get; set; } = string.Empty;

        [StringLength(450)]
        public string? VerifiedById { get; set; }

        public DateTime? VerifiedAt { get; set; }

        // Navigation properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual User CreatedBy { get; set; } = null!;
        public virtual User? VerifiedBy { get; set; }
        public virtual ICollection<PurchaseReceiptItem> Items { get; set; } = new List<PurchaseReceiptItem>();
    }
}

