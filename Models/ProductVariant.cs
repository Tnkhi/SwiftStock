using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // Ex: "Rouge", "L", "500ml"

        [StringLength(50)]
        public string? AttributeType { get; set; } // Ex: "Couleur", "Taille", "Volume"

        [StringLength(100)]
        public string? SKU { get; set; }

        [StringLength(50)]
        public string? Barcode { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PurchasePrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? SalePrice { get; set; }

        public int CurrentStock { get; set; } = 0;
        public int MinStockLevel { get; set; } = 0;
        public int MaxStockLevel { get; set; } = 1000;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? CreatedById { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual User? CreatedBy { get; set; }
        public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}

