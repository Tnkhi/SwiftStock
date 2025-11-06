using SwiftStock.Models;

namespace SwiftStock.Services
{
    public interface IProductService
    {
        // Gestion des variantes
        Task<ProductVariant> CreateVariantAsync(ProductVariant variant);
        Task<ProductVariant?> GetVariantAsync(int variantId);
        Task<IEnumerable<ProductVariant>> GetProductVariantsAsync(int productId);
        Task<ProductVariant> UpdateVariantAsync(ProductVariant variant);
        Task<bool> DeleteVariantAsync(int variantId);

        // Gestion des catégories
        Task<ProductCategory> CreateCategoryAsync(ProductCategory category);
        Task<ProductCategory?> GetCategoryAsync(int categoryId);
        Task<IEnumerable<ProductCategory>> GetCategoriesAsync(bool includeInactive = false);
        Task<IEnumerable<ProductCategory>> GetSubCategoriesAsync(int parentCategoryId);
        Task<ProductCategory> UpdateCategoryAsync(ProductCategory category);
        Task<bool> DeleteCategoryAsync(int categoryId);

        // Gestion des ajustements de stock
        Task<StockAdjustment> CreateAdjustmentAsync(StockAdjustment adjustment);
        Task<StockAdjustment?> GetAdjustmentAsync(int adjustmentId);
        Task<IEnumerable<StockAdjustment>> GetProductAdjustmentsAsync(int productId);
        Task<IEnumerable<StockAdjustment>> GetPendingAdjustmentsAsync();
        Task<bool> ApproveAdjustmentAsync(int adjustmentId, string approvedById);
        Task<bool> RejectAdjustmentAsync(int adjustmentId, string rejectedById);

        // Gestion des inventaires physiques
        Task<PhysicalInventory> CreateInventoryAsync(PhysicalInventory inventory);
        Task<PhysicalInventory?> GetInventoryAsync(int inventoryId);
        Task<IEnumerable<PhysicalInventory>> GetInventoriesAsync();
        Task<PhysicalInventory> StartInventoryAsync(int inventoryId);
        Task<PhysicalInventory> CompleteInventoryAsync(int inventoryId);
        Task<bool> CancelInventoryAsync(int inventoryId);

        // Gestion des items d'inventaire
        Task<PhysicalInventoryItem> CreateInventoryItemAsync(PhysicalInventoryItem item);
        Task<PhysicalInventoryItem?> GetInventoryItemAsync(int itemId);
        Task<IEnumerable<PhysicalInventoryItem>> GetInventoryItemsAsync(int inventoryId);
        Task<PhysicalInventoryItem> UpdateInventoryItemCountAsync(int itemId, int countedStock, string countedById);
        Task<bool> VerifyInventoryItemAsync(int itemId, string verifiedById);

        // Codes-barres
        Task<string> GenerateBarcodeAsync();
        Task<bool> ValidateBarcodeAsync(string barcode);
        Task<Product?> GetProductByBarcodeAsync(string barcode);
        Task<ProductVariant?> GetVariantByBarcodeAsync(string barcode);

        // Statistiques
        Task<object> GetProductStatisticsAsync(int productId);
        Task<object> GetInventoryStatisticsAsync();
    }
}

