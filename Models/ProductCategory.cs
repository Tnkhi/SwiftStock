using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class ProductCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Code { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        public int? ParentCategoryId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? CreatedById { get; set; }

        // Navigation properties
        public virtual ProductCategory? ParentCategory { get; set; }
        public virtual ICollection<ProductCategory> SubCategories { get; set; } = new List<ProductCategory>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual User? CreatedBy { get; set; }
    }
}

