using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public enum PaymentMethod
    {
        Cash,           // Espèces
        Card,           // Carte bancaire
        MobileMoney,    // Mobile Money
        Credit,         // Crédit
        Check           // Chèque
    }

    public enum SaleStatus
    {
        Completed,      // Terminée
        Cancelled,      // Annulée
        Refunded        // Remboursée
    }

    public class Sale
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string SaleNumber { get; set; } = string.Empty;

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public SaleStatus Status { get; set; } = SaleStatus.Completed;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ChangeAmount { get; set; } = 0;

        [StringLength(200)]
        public string? CustomerName { get; set; }

        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        public int? CustomerId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? CashierId { get; set; }

        // Navigation properties
        public virtual User? Cashier { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        public virtual ICollection<PromotionUsage> PromotionUsages { get; set; } = new List<PromotionUsage>();
    }
}

