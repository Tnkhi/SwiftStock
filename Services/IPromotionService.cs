using SwiftStock.Models;

namespace SwiftStock.Services
{
    public interface IPromotionService
    {
        // Gestion des promotions
        Task<Promotion> CreatePromotionAsync(Promotion promotion);
        Task<Promotion?> GetPromotionAsync(int promotionId);
        Task<IEnumerable<Promotion>> GetActivePromotionsAsync();
        Task<IEnumerable<Promotion>> GetPromotionsAsync();
        Task<Promotion> UpdatePromotionAsync(Promotion promotion);
        Task<bool> DeletePromotionAsync(int promotionId);

        // Gestion des produits de promotion
        Task AddProductToPromotionAsync(int promotionId, int productId, int? variantId = null);
        Task RemoveProductFromPromotionAsync(int promotionId, int productId, int? variantId = null);
        Task<IEnumerable<Product>> GetPromotionProductsAsync(int promotionId);

        // Application des promotions
        Task<decimal> CalculateDiscountAsync(int promotionId, int productId, int? variantId, int quantity, decimal unitPrice);
        Task<bool> CanApplyPromotionAsync(int promotionId, int productId, int? variantId, int quantity, decimal totalAmount);
        Task<PromotionUsage> ApplyPromotionAsync(int promotionId, int saleId, decimal discountAmount, string? promoCode = null, string? usedById = null);

        // Validation des codes promo
        Task<bool> ValidatePromoCodeAsync(string promoCode);
        Task<Promotion?> GetPromotionByCodeAsync(string promoCode);

        // Statistiques
        Task<object> GetPromotionStatisticsAsync(int promotionId);
        Task<object> GetPromotionUsageStatisticsAsync();
    }
}

