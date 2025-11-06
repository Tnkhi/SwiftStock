using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class PromotionProduct
    {
        public int Id { get; set; }

        [Required]
        public int PromotionId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

        // Navigation properties
        public virtual Promotion Promotion { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ProductVariant? ProductVariant { get; set; }
    }
}

