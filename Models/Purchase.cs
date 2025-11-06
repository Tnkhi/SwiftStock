using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public enum PurchaseStatus
    {
        Pending,        // En attente
        Ordered,        // Commandé
        Received,       // Reçu
        Cancelled       // Annulé
    }

    public class Purchase
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PurchaseNumber { get; set; } = string.Empty;

        [Required]
        public int SupplierId { get; set; }

        [Required]
        public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? CreatedById { get; set; }

        // Navigation properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual User? CreatedBy { get; set; }
        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}

