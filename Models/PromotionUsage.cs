using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class PromotionUsage
    {
        public int Id { get; set; }

        [Required]
        public int PromotionId { get; set; }

        [Required]
        public int SaleId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; }

        [StringLength(50)]
        public string? PromoCode { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? UsedById { get; set; }

        // Navigation properties
        public virtual Promotion Promotion { get; set; } = null!;
        public virtual Sale Sale { get; set; } = null!;
        public virtual User? UsedBy { get; set; }
    }
}

