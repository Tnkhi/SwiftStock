using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public enum PromotionType
    {
        Percentage,     // Pourcentage de réduction
        FixedAmount,    // Montant fixe de réduction
        BuyXGetY,       // Acheter X, obtenir Y gratuit
        FreeShipping,   // Livraison gratuite
        Bundle          // Offre groupée
    }

    public enum PromotionStatus
    {
        Draft,          // Brouillon
        Active,         // Actif
        Paused,         // En pause
        Expired,        // Expiré
        Cancelled       // Annulé
    }

    public class Promotion
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public PromotionType Type { get; set; }

        [Required]
        public PromotionStatus Status { get; set; } = PromotionStatus.Draft;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountPercentage { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DiscountAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MinimumPurchaseAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaximumDiscountAmount { get; set; }

        public int? BuyQuantity { get; set; }
        public int? GetQuantity { get; set; }

        [StringLength(50)]
        public string? PromoCode { get; set; }

        public bool RequirePromoCode { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool ApplyToAllProducts { get; set; } = true;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string CreatedById { get; set; } = string.Empty;

        // Navigation properties
        public virtual User CreatedBy { get; set; } = null!;
        public virtual ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();
        public virtual ICollection<PromotionUsage> PromotionUsages { get; set; } = new List<PromotionUsage>();
    }
}

