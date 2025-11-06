using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Gestion des variantes

        public async Task<ProductVariant> CreateVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();
            return variant;
        }

        public async Task<ProductVariant?> GetVariantAsync(int variantId)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product)
                .Include(pv => pv.CreatedBy)
                .FirstOrDefaultAsync(pv => pv.Id == variantId);
        }

        public async Task<IEnumerable<ProductVariant>> GetProductVariantsAsync(int productId)
        {
            return await _context.ProductVariants
                .Where(pv => pv.ProductId == productId && pv.IsActive)
                .OrderBy(pv => pv.Name)
                .ToListAsync();
        }

        public async Task<ProductVariant> UpdateVariantAsync(ProductVariant variant)
        {
            variant.UpdatedAt = DateTime.UtcNow;
            _context.ProductVariants.Update(variant);
            await _context.SaveChangesAsync();
            return variant;
        }

        public async Task<bool> DeleteVariantAsync(int variantId)
        {
            var variant = await _context.ProductVariants.FindAsync(variantId);
            if (variant == null) return false;

            variant.IsActive = false;
            variant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Gestion des catégories

        public async Task<ProductCategory> CreateCategoryAsync(ProductCategory category)
        {
            _context.ProductCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<ProductCategory?> GetCategoryAsync(int categoryId)
        {
            return await _context.ProductCategories
                .Include(pc => pc.ParentCategory)
                .Include(pc => pc.SubCategories)
                .Include(pc => pc.CreatedBy)
                .FirstOrDefaultAsync(pc => pc.Id == categoryId);
        }

        public async Task<IEnumerable<ProductCategory>> GetCategoriesAsync(bool includeInactive = false)
        {
            var query = _context.ProductCategories.AsQueryable();
            
            if (!includeInactive)
                query = query.Where(pc => pc.IsActive);

            return await query
                .Where(pc => pc.ParentCategoryId == null)
                .Include(pc => pc.SubCategories.Where(sc => includeInactive || sc.IsActive))
                .OrderBy(pc => pc.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductCategory>> GetSubCategoriesAsync(int parentCategoryId)
        {
            return await _context.ProductCategories
                .Where(pc => pc.ParentCategoryId == parentCategoryId && pc.IsActive)
                .OrderBy(pc => pc.Name)
                .ToListAsync();
        }

        public async Task<ProductCategory> UpdateCategoryAsync(ProductCategory category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            _context.ProductCategories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var category = await _context.ProductCategories.FindAsync(categoryId);
            if (category == null) return false;

            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Gestion des ajustements de stock

        public async Task<StockAdjustment> CreateAdjustmentAsync(StockAdjustment adjustment)
        {
            _context.StockAdjustments.Add(adjustment);
            await _context.SaveChangesAsync();
            return adjustment;
        }

        public async Task<StockAdjustment?> GetAdjustmentAsync(int adjustmentId)
        {
            return await _context.StockAdjustments
                .Include(sa => sa.Product)
                .Include(sa => sa.ProductVariant)
                .Include(sa => sa.CreatedBy)
                .Include(sa => sa.ApprovedBy)
                .FirstOrDefaultAsync(sa => sa.Id == adjustmentId);
        }

        public async Task<IEnumerable<StockAdjustment>> GetProductAdjustmentsAsync(int productId)
        {
            return await _context.StockAdjustments
                .Where(sa => sa.ProductId == productId)
                .Include(sa => sa.ProductVariant)
                .Include(sa => sa.CreatedBy)
                .Include(sa => sa.ApprovedBy)
                .OrderByDescending(sa => sa.AdjustmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockAdjustment>> GetPendingAdjustmentsAsync()
        {
            return await _context.StockAdjustments
                .Where(sa => sa.Status == AdjustmentStatus.Pending)
                .Include(sa => sa.Product)
                .Include(sa => sa.ProductVariant)
                .Include(sa => sa.CreatedBy)
                .OrderByDescending(sa => sa.AdjustmentDate)
                .ToListAsync();
        }

        public async Task<bool> ApproveAdjustmentAsync(int adjustmentId, string approvedById)
        {
            var adjustment = await _context.StockAdjustments.FindAsync(adjustmentId);
            if (adjustment == null || adjustment.Status != AdjustmentStatus.Pending) return false;

            adjustment.Status = AdjustmentStatus.Approved;
            adjustment.ApprovedAt = DateTime.UtcNow;
            adjustment.ApprovedById = approvedById;

            // Appliquer l'ajustement au stock
            var product = await _context.Products.FindAsync(adjustment.ProductId);
            if (product != null)
            {
                product.CurrentStock = adjustment.NewStock;
                product.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAdjustmentAsync(int adjustmentId, string rejectedById)
        {
            var adjustment = await _context.StockAdjustments.FindAsync(adjustmentId);
            if (adjustment == null || adjustment.Status != AdjustmentStatus.Pending) return false;

            adjustment.Status = AdjustmentStatus.Rejected;
            adjustment.ApprovedAt = DateTime.UtcNow;
            adjustment.ApprovedById = rejectedById;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Gestion des inventaires physiques

        public async Task<PhysicalInventory> CreateInventoryAsync(PhysicalInventory inventory)
        {
            _context.PhysicalInventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<PhysicalInventory?> GetInventoryAsync(int inventoryId)
        {
            return await _context.PhysicalInventories
                .Include(pi => pi.CreatedBy)
                .Include(pi => pi.Items)
                    .ThenInclude(pii => pii.Product)
                .Include(pi => pi.Items)
                    .ThenInclude(pii => pii.ProductVariant)
                .FirstOrDefaultAsync(pi => pi.Id == inventoryId);
        }

        public async Task<IEnumerable<PhysicalInventory>> GetInventoriesAsync()
        {
            return await _context.PhysicalInventories
                .Include(pi => pi.CreatedBy)
                .OrderByDescending(pi => pi.CreatedAt)
                .ToListAsync();
        }

        public async Task<PhysicalInventory> StartInventoryAsync(int inventoryId)
        {
            var inventory = await _context.PhysicalInventories.FindAsync(inventoryId);
            if (inventory == null) throw new ArgumentException("Inventaire non trouvé");

            inventory.Status = InventoryStatus.InProgress;
            inventory.StartDate = DateTime.UtcNow;
            inventory.UpdatedAt = DateTime.UtcNow;

            // Créer les items d'inventaire pour tous les produits actifs
            var products = await _context.Products
                .Where(p => p.IsActive && (inventory.IncludeInactiveProducts || p.IsActive))
                .ToListAsync();

            foreach (var product in products)
            {
                var item = new PhysicalInventoryItem
                {
                    PhysicalInventoryId = inventoryId,
                    ProductId = product.Id,
                    SystemStock = product.CurrentStock,
                    UnitCost = product.PurchasePrice,
                    Status = CountStatus.NotCounted
                };
                _context.PhysicalInventoryItems.Add(item);
            }

            inventory.TotalProducts = products.Count;
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<PhysicalInventory> CompleteInventoryAsync(int inventoryId)
        {
            var inventory = await _context.PhysicalInventories.FindAsync(inventoryId);
            if (inventory == null) throw new ArgumentException("Inventaire non trouvé");

            inventory.Status = InventoryStatus.Completed;
            inventory.EndDate = DateTime.UtcNow;
            inventory.UpdatedAt = DateTime.UtcNow;

            // Calculer les statistiques
            var items = await _context.PhysicalInventoryItems
                .Where(pii => pii.PhysicalInventoryId == inventoryId)
                .ToListAsync();

            inventory.CountedProducts = items.Count(pii => pii.Status == CountStatus.Counted || pii.Status == CountStatus.Discrepancy);
            inventory.Discrepancies = items.Count(pii => pii.Status == CountStatus.Discrepancy);
            inventory.TotalValue = items.Sum(pii => pii.SystemStock * pii.UnitCost);
            inventory.DiscrepancyValue = items.Sum(pii => pii.DiscrepancyValue);

            // Appliquer les ajustements si demandé
            if (inventory.AutoAdjustStock)
            {
                foreach (var item in items.Where(pii => pii.Status == CountStatus.Discrepancy && pii.Discrepancy.HasValue))
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.CurrentStock = item.CountedStock ?? product.CurrentStock;
                        product.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<bool> CancelInventoryAsync(int inventoryId)
        {
            var inventory = await _context.PhysicalInventories.FindAsync(inventoryId);
            if (inventory == null) return false;

            inventory.Status = InventoryStatus.Cancelled;
            inventory.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Gestion des items d'inventaire

        public async Task<PhysicalInventoryItem> CreateInventoryItemAsync(PhysicalInventoryItem item)
        {
            _context.PhysicalInventoryItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<PhysicalInventoryItem?> GetInventoryItemAsync(int itemId)
        {
            return await _context.PhysicalInventoryItems
                .Include(pii => pii.Product)
                .Include(pii => pii.ProductVariant)
                .Include(pii => pii.CountedBy)
                .Include(pii => pii.VerifiedBy)
                .FirstOrDefaultAsync(pii => pii.Id == itemId);
        }

        public async Task<IEnumerable<PhysicalInventoryItem>> GetInventoryItemsAsync(int inventoryId)
        {
            return await _context.PhysicalInventoryItems
                .Where(pii => pii.PhysicalInventoryId == inventoryId)
                .Include(pii => pii.Product)
                .Include(pii => pii.ProductVariant)
                .Include(pii => pii.CountedBy)
                .OrderBy(pii => pii.Product.Name)
                .ToListAsync();
        }

        public async Task<PhysicalInventoryItem> UpdateInventoryItemCountAsync(int itemId, int countedStock, string countedById)
        {
            var item = await _context.PhysicalInventoryItems.FindAsync(itemId);
            if (item == null) throw new ArgumentException("Item d'inventaire non trouvé");

            item.CountedStock = countedStock;
            item.Discrepancy = countedStock - item.SystemStock;
            item.DiscrepancyValue = item.Discrepancy.Value * item.UnitCost;
            item.Status = item.Discrepancy == 0 ? CountStatus.Counted : CountStatus.Discrepancy;
            item.CountedAt = DateTime.UtcNow;
            item.CountedById = countedById;

            _context.PhysicalInventoryItems.Update(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> VerifyInventoryItemAsync(int itemId, string verifiedById)
        {
            var item = await _context.PhysicalInventoryItems.FindAsync(itemId);
            if (item == null) return false;

            item.Status = CountStatus.Verified;
            item.VerifiedAt = DateTime.UtcNow;
            item.VerifiedById = verifiedById;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Codes-barres

        public async Task<string> GenerateBarcodeAsync()
        {
            // Génération d'un code-barres EAN-13 simple
            var random = new Random();
            var barcode = "200" + random.Next(100000000, 999999999).ToString();
            
            // Vérifier l'unicité
            while (await _context.Products.AnyAsync(p => p.Barcode == barcode) ||
                   await _context.ProductVariants.AnyAsync(pv => pv.Barcode == barcode))
            {
                barcode = "200" + random.Next(100000000, 999999999).ToString();
            }

            return barcode;
        }

        public async Task<bool> ValidateBarcodeAsync(string barcode)
        {
            if (string.IsNullOrEmpty(barcode) || barcode.Length != 13) return false;
            
            // Vérifier l'unicité
            return !await _context.Products.AnyAsync(p => p.Barcode == barcode) &&
                   !await _context.ProductVariants.AnyAsync(pv => pv.Barcode == barcode);
        }

        public async Task<Product?> GetProductByBarcodeAsync(string barcode)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Barcode == barcode && p.IsActive);
        }

        public async Task<ProductVariant?> GetVariantByBarcodeAsync(string barcode)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Barcode == barcode && pv.IsActive);
        }

        #endregion

        #region Statistiques

        public async Task<object> GetProductStatisticsAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.StockMovements)
                .Include(p => p.SaleItems)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return new { };

            var totalSales = await _context.SaleItems
                .Where(si => si.ProductId == productId)
                .SumAsync(si => si.Quantity);

            var totalRevenue = await _context.SaleItems
                .Where(si => si.ProductId == productId)
                .SumAsync(si => si.TotalPrice);

            return new
            {
                ProductId = productId,
                ProductName = product.Name,
                CurrentStock = product.CurrentStock,
                TotalVariants = product.Variants.Count,
                TotalSales = totalSales,
                TotalRevenue = totalRevenue,
                LastMovement = product.StockMovements.OrderByDescending(sm => sm.CreatedAt).FirstOrDefault()?.CreatedAt,
                LowStock = product.CurrentStock <= product.MinStockLevel
            };
        }

        public async Task<object> GetInventoryStatisticsAsync()
        {
            var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
            var lowStockProducts = await _context.Products.CountAsync(p => p.IsActive && p.CurrentStock <= p.MinStockLevel);
            var outOfStockProducts = await _context.Products.CountAsync(p => p.IsActive && p.CurrentStock <= 0);
            var totalValue = await _context.Products
                .Where(p => p.IsActive)
                .SumAsync(p => p.CurrentStock * p.PurchasePrice);

            return new
            {
                TotalProducts = totalProducts,
                LowStockProducts = lowStockProducts,
                OutOfStockProducts = outOfStockProducts,
                TotalValue = totalValue,
                LowStockPercentage = totalProducts > 0 ? (double)lowStockProducts / totalProducts * 100 : 0
            };
        }

        #endregion
    }
}

