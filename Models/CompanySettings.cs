using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class CompanySettings
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? LegalName { get; set; }

        [StringLength(50)]
        public string? TaxNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(50)]
        public string? Country { get; set; } = "Côte d'Ivoire";

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        // Paramètres de devise
        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "FCFA";

        [StringLength(10)]
        public string CurrencySymbol { get; set; } = "F";

        // Paramètres de taxes
        [Column(TypeName = "decimal(5,2)")]
        public decimal DefaultTaxRate { get; set; } = 18.00m;

        [StringLength(50)]
        public string TaxName { get; set; } = "TVA";

        // Paramètres d'impression
        [StringLength(200)]
        public string? ReceiptHeader { get; set; }

        [StringLength(200)]
        public string? ReceiptFooter { get; set; }

        public bool PrintReceipt { get; set; } = true;
        public bool PrintInvoice { get; set; } = false;

        // Paramètres de stock
        public bool EnableLowStockAlerts { get; set; } = true;
        public int DefaultLowStockThreshold { get; set; } = 10;

        // Paramètres de vente
        public bool RequireCustomerInfo { get; set; } = false;
        public bool EnableDiscounts { get; set; } = true;
        public decimal MaxDiscountPercentage { get; set; } = 50.00m;

        // Paramètres de sécurité
        public bool RequirePasswordForVoid { get; set; } = true;
        public bool EnableAuditLog { get; set; } = true;
        public int SessionTimeoutMinutes { get; set; } = 480; // 8 heures

        // Paramètres de sauvegarde
        public bool AutoBackup { get; set; } = true;
        public int BackupFrequencyDays { get; set; } = 7;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? UpdatedById { get; set; }

        // Navigation properties
        public virtual User? UpdatedBy { get; set; }
    }
}

