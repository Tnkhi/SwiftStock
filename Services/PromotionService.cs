using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PromotionService> _logger;

        public PromotionService(ApplicationDbContext context, ILogger<PromotionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Gestion des promotions

        public async Task<Promotion> CreatePromotionAsync(Promotion promotion)
        {
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<Promotion?> GetPromotionAsync(int promotionId)
        {
            return await _context.Promotions
                .Include(p => p.CreatedBy)
                .Include(p => p.PromotionProducts)
                    .ThenInclude(pp => pp.Product)
                .Include(p => p.PromotionProducts)
                    .ThenInclude(pp => pp.ProductVariant)
                .Include(p => p.PromotionUsages)
                .FirstOrDefaultAsync(p => p.Id == promotionId);
        }

        public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Promotions
                .Where(p => p.IsActive && 
                           p.Status == PromotionStatus.Active &&
                           p.StartDate <= now && 
                           p.EndDate >= now)
                .Include(p => p.PromotionProducts)
                    .ThenInclude(pp => pp.Product)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Promotion>> GetPromotionsAsync()
        {
            return await _context.Promotions
                .Include(p => p.CreatedBy)
                .Include(p => p.PromotionProducts)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Promotion> UpdatePromotionAsync(Promotion promotion)
        {
            promotion.UpdatedAt = DateTime.UtcNow;
            _context.Promotions.Update(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<bool> DeletePromotionAsync(int promotionId)
        {
            var promotion = await _context.Promotions.FindAsync(promotionId);
            if (promotion == null) return false;

            promotion.IsActive = false;
            promotion.Status = PromotionStatus.Cancelled;
            promotion.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Gestion des produits de promotion

        public async Task AddProductToPromotionAsync(int promotionId, int productId, int? variantId = null)
        {
            var existingProduct = await _context.PromotionProducts
                .FirstOrDefaultAsync(pp => pp.PromotionId == promotionId && 
                                          pp.ProductId == productId && 
                                          pp.ProductVariantId == variantId);

            if (existingProduct == null)
            {
                var promotionProduct = new PromotionProduct
                {
                    PromotionId = promotionId,
                    ProductId = productId,
                    ProductVariantId = variantId
                };

                _context.PromotionProducts.Add(promotionProduct);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveProductFromPromotionAsync(int promotionId, int productId, int? variantId = null)
        {
            var promotionProduct = await _context.PromotionProducts
                .FirstOrDefaultAsync(pp => pp.PromotionId == promotionId && 
                                          pp.ProductId == productId && 
                                          pp.ProductVariantId == variantId);

            if (promotionProduct != null)
            {
                _context.PromotionProducts.Remove(promotionProduct);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Product>> GetPromotionProductsAsync(int promotionId)
        {
            return await _context.PromotionProducts
                .Where(pp => pp.PromotionId == promotionId)
                .Include(pp => pp.Product)
                .Include(pp => pp.ProductVariant)
                .Select(pp => pp.Product)
                .Distinct()
                .ToListAsync();
        }

        #endregion

        #region Application des promotions

        public async Task<decimal> CalculateDiscountAsync(int promotionId, int productId, int? variantId, int quantity, decimal unitPrice)
        {
            var promotion = await _context.Promotions
                .Include(p => p.PromotionProducts)
                .FirstOrDefaultAsync(p => p.Id == promotionId);

            if (promotion == null || !promotion.IsActive || promotion.Status != PromotionStatus.Active)
                return 0;

            var now = DateTime.UtcNow;
            if (promotion.StartDate > now || promotion.EndDate < now)
                return 0;

            // Vérifier si le produit est dans la promotion
            if (!promotion.ApplyToAllProducts)
            {
                var isProductInPromotion = await _context.PromotionProducts
                    .AnyAsync(pp => pp.PromotionId == promotionId && 
                                   pp.ProductId == productId && 
                                   pp.ProductVariantId == variantId);

                if (!isProductInPromotion)
                    return 0;
            }

            var totalAmount = quantity * unitPrice;

            // Vérifier le montant minimum d'achat
            if (promotion.MinimumPurchaseAmount.HasValue && totalAmount < promotion.MinimumPurchaseAmount.Value)
                return 0;

            decimal discount = 0;

            switch (promotion.Type)
            {
                case PromotionType.Percentage:
                    if (promotion.DiscountPercentage.HasValue)
                    {
                        discount = totalAmount * (promotion.DiscountPercentage.Value / 100);
                    }
                    break;

                case PromotionType.FixedAmount:
                    if (promotion.DiscountAmount.HasValue)
                    {
                        discount = promotion.DiscountAmount.Value;
                    }
                    break;

                case PromotionType.BuyXGetY:
                    if (promotion.BuyQuantity.HasValue && promotion.GetQuantity.HasValue)
                    {
                        var freeItems = (quantity / promotion.BuyQuantity.Value) * promotion.GetQuantity.Value;
                        discount = Math.Min(freeItems, quantity) * unitPrice;
                    }
                    break;
            }

            // Appliquer le montant maximum de réduction
            if (promotion.MaximumDiscountAmount.HasValue && discount > promotion.MaximumDiscountAmount.Value)
            {
                discount = promotion.MaximumDiscountAmount.Value;
            }

            return Math.Min(discount, totalAmount);
        }

        public async Task<bool> CanApplyPromotionAsync(int promotionId, int productId, int? variantId, int quantity, decimal totalAmount)
        {
            var promotion = await _context.Promotions.FindAsync(promotionId);
            if (promotion == null || !promotion.IsActive || promotion.Status != PromotionStatus.Active)
                return false;

            var now = DateTime.UtcNow;
            if (promotion.StartDate > now || promotion.EndDate < now)
                return false;

            // Vérifier le montant minimum d'achat
            if (promotion.MinimumPurchaseAmount.HasValue && totalAmount < promotion.MinimumPurchaseAmount.Value)
                return false;

            // Vérifier si le produit est dans la promotion
            if (!promotion.ApplyToAllProducts)
            {
                var isProductInPromotion = await _context.PromotionProducts
                    .AnyAsync(pp => pp.PromotionId == promotionId && 
                                   pp.ProductId == productId && 
                                   pp.ProductVariantId == variantId);

                if (!isProductInPromotion)
                    return false;
            }

            return true;
        }

        public async Task<PromotionUsage> ApplyPromotionAsync(int promotionId, int saleId, decimal discountAmount, string? promoCode = null, string? usedById = null)
        {
            var promotionUsage = new PromotionUsage
            {
                PromotionId = promotionId,
                SaleId = saleId,
                DiscountAmount = discountAmount,
                PromoCode = promoCode,
                UsedAt = DateTime.UtcNow,
                UsedById = usedById
            };

            _context.PromotionUsages.Add(promotionUsage);
            await _context.SaveChangesAsync();
            return promotionUsage;
        }

        #endregion

        #region Validation des codes promo

        public async Task<bool> ValidatePromoCodeAsync(string promoCode)
        {
            if (string.IsNullOrEmpty(promoCode))
                return false;

            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.PromoCode == promoCode && 
                                         p.IsActive && 
                                         p.Status == PromotionStatus.Active &&
                                         p.RequirePromoCode);

            if (promotion == null)
                return false;

            var now = DateTime.UtcNow;
            return promotion.StartDate <= now && promotion.EndDate >= now;
        }

        public async Task<Promotion?> GetPromotionByCodeAsync(string promoCode)
        {
            if (string.IsNullOrEmpty(promoCode))
                return null;

            return await _context.Promotions
                .Include(p => p.PromotionProducts)
                    .ThenInclude(pp => pp.Product)
                .FirstOrDefaultAsync(p => p.PromoCode == promoCode && 
                                         p.IsActive && 
                                         p.Status == PromotionStatus.Active);
        }

        #endregion

        #region Statistiques

        public async Task<object> GetPromotionStatisticsAsync(int promotionId)
        {
            var promotion = await _context.Promotions.FindAsync(promotionId);
            if (promotion == null) return new { };

            var totalUsage = await _context.PromotionUsages
                .Where(pu => pu.PromotionId == promotionId)
                .CountAsync();

            var totalDiscount = await _context.PromotionUsages
                .Where(pu => pu.PromotionId == promotionId)
                .SumAsync(pu => pu.DiscountAmount);

            var uniqueCustomers = await _context.PromotionUsages
                .Where(pu => pu.PromotionId == promotionId)
                .Include(pu => pu.Sale)
                .Select(pu => pu.Sale.CustomerId)
                .Where(customerId => customerId.HasValue)
                .Distinct()
                .CountAsync();

            return new
            {
                PromotionId = promotionId,
                PromotionName = promotion.Name,
                TotalUsage = totalUsage,
                TotalDiscount = totalDiscount,
                UniqueCustomers = uniqueCustomers,
                AverageDiscount = totalUsage > 0 ? totalDiscount / totalUsage : 0
            };
        }

        public async Task<object> GetPromotionUsageStatisticsAsync()
        {
            var totalPromotions = await _context.Promotions.CountAsync(p => p.IsActive);
            var activePromotions = await _context.Promotions
                .CountAsync(p => p.IsActive && p.Status == PromotionStatus.Active);

            var totalUsage = await _context.PromotionUsages.CountAsync();
            var totalDiscount = await _context.PromotionUsages.SumAsync(pu => pu.DiscountAmount);

            var todayUsage = await _context.PromotionUsages
                .Where(pu => pu.UsedAt.Date == DateTime.Today)
                .CountAsync();

            var todayDiscount = await _context.PromotionUsages
                .Where(pu => pu.UsedAt.Date == DateTime.Today)
                .SumAsync(pu => pu.DiscountAmount);

            return new
            {
                TotalPromotions = totalPromotions,
                ActivePromotions = activePromotions,
                TotalUsage = totalUsage,
                TotalDiscount = totalDiscount,
                TodayUsage = todayUsage,
                TodayDiscount = todayDiscount,
                AverageDiscount = totalUsage > 0 ? totalDiscount / totalUsage : 0
            };
        }

        #endregion
    }
}

