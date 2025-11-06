using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? SKU { get; set; }

        [StringLength(50)]
        public string? Barcode { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TaxRate { get; set; } = 0;

        [StringLength(50)]
        public string? CategoryName { get; set; }

        [StringLength(50)]
        public string? Brand { get; set; }

        [StringLength(20)]
        public string? Unit { get; set; } = "pcs"; // pcs, kg, L, etc.

        public int MinStockLevel { get; set; } = 0;
        public int MaxStockLevel { get; set; } = 1000;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations avec les nouveaux modèles
        public int? CategoryId { get; set; }

        // Navigation properties
        public virtual ProductCategory? Category { get; set; }
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        public virtual ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();
        public virtual ICollection<PhysicalInventoryItem> PhysicalInventoryItems { get; set; } = new List<PhysicalInventoryItem>();
        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();

        public int CurrentStock { get; set; } = 0;

        [NotMapped]
        public bool IsLowStock => CurrentStock <= MinStockLevel;

        [NotMapped]
        public decimal ProfitMargin => SalePrice - PurchasePrice;
    }
}

