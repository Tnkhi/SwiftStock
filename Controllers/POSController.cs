using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;
using SwiftStock.Services;

namespace SwiftStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public class POSController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPromotionService _promotionService;
        private readonly IAuditService _auditService;
        private readonly ILogger<POSController> _logger;

        public POSController(
            ApplicationDbContext context, 
            IPromotionService promotionService,
            IAuditService auditService,
            ILogger<POSController> logger)
        {
            _context = context;
            _promotionService = promotionService;
            _auditService = auditService;
            _logger = logger;
        }

        #region Gestion des promotions

        [HttpPost("promotions")]
        public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
        {
            try
            {
                var promotion = new Promotion
                {
                    Name = request.Name,
                    Description = request.Description,
                    Type = request.Type,
                    DiscountPercentage = request.DiscountPercentage,
                    DiscountAmount = request.DiscountAmount,
                    MinimumPurchaseAmount = request.MinimumPurchaseAmount,
                    MaximumDiscountAmount = request.MaximumDiscountAmount,
                    BuyQuantity = request.BuyQuantity,
                    GetQuantity = request.GetQuantity,
                    PromoCode = request.PromoCode,
                    RequirePromoCode = request.RequirePromoCode,
                    ApplyToAllProducts = request.ApplyToAllProducts,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = PromotionStatus.Draft,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = User.Identity?.Name ?? ""
                };

                var createdPromotion = await _promotionService.CreatePromotionAsync(promotion);

                // Ajouter les produits à la promotion si spécifiés
                if (request.ProductIds?.Any() == true)
                {
                    foreach (var productId in request.ProductIds)
                    {
                        await _promotionService.AddProductToPromotionAsync(createdPromotion.Id, productId);
                    }
                }

                // Enregistrer l'audit
                await _auditService.LogAsync(
                    AuditAction.Create,
                    "Promotion",
                    createdPromotion.Id.ToString(),
                    createdPromotion.Name,
                    "Promotion créée",
                    null,
                    $"Type: {createdPromotion.Type}, Réduction: {createdPromotion.DiscountPercentage}%",
                    User.Identity?.Name,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );

                return Ok(createdPromotion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la promotion: {PromotionName}", request.Name);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpGet("promotions")]
        public async Task<IActionResult> GetPromotions([FromQuery] bool activeOnly = false)
        {
            try
            {
                var promotions = activeOnly 
                    ? await _promotionService.GetActivePromotionsAsync()
                    : await _promotionService.GetPromotionsAsync();

                return Ok(promotions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des promotions");
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpGet("promotions/{promotionId}")]
        public async Task<IActionResult> GetPromotion(int promotionId)
        {
            try
            {
                var promotion = await _promotionService.GetPromotionAsync(promotionId);
                if (promotion == null)
                {
                    return NotFound("Promotion non trouvée");
                }

                return Ok(promotion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la promotion {PromotionId}", promotionId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpPost("promotions/{promotionId}/toggle-status")]
        public async Task<IActionResult> TogglePromotionStatus(int promotionId, [FromQuery] string status)
        {
            try
            {
                var promotion = await _context.Promotions.FindAsync(promotionId);
                if (promotion == null)
                {
                    return NotFound("Promotion non trouvée");
                }

                if (!Enum.TryParse<PromotionStatus>(status, out var newStatus))
                {
                    return BadRequest("Statut invalide");
                }

                var oldStatus = promotion.Status;
                promotion.Status = newStatus;
                promotion.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Enregistrer l'audit
                await _auditService.LogAsync(
                    AuditAction.Update,
                    "Promotion",
                    promotionId.ToString(),
                    promotion.Name,
                    $"Statut changé de {oldStatus} à {newStatus}",
                    oldStatus.ToString(),
                    newStatus.ToString(),
                    User.Identity?.Name,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );

                _logger.LogInformation("Statut de la promotion {PromotionId} changé à {Status}", promotionId, newStatus);
                return Ok(new { success = true, message = "Statut mis à jour avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de statut de la promotion {PromotionId}", promotionId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpDelete("promotions/{promotionId}")]
        public async Task<IActionResult> DeletePromotion(int promotionId)
        {
            try
            {
                var success = await _promotionService.DeletePromotionAsync(promotionId);
                if (!success)
                {
                    return NotFound("Promotion non trouvée");
                }

                // Enregistrer l'audit
                await _auditService.LogAsync(
                    AuditAction.Delete,
                    "Promotion",
                    promotionId.ToString(),
                    "Promotion supprimée",
                    "Promotion supprimée",
                    null,
                    null,
                    User.Identity?.Name,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );

                return Ok(new { success = true, message = "Promotion supprimée avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la promotion {PromotionId}", promotionId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        #endregion

        #region Application des promotions

        [HttpPost("promotions/validate-code")]
        public async Task<IActionResult> ValidatePromoCode([FromBody] ValidatePromoCodeRequest request)
        {
            try
            {
                var isValid = await _promotionService.ValidatePromoCodeAsync(request.PromoCode);
                if (!isValid)
                {
                    return Ok(new { isValid = false, message = "Code promo invalide ou expiré" });
                }

                var promotion = await _promotionService.GetPromotionByCodeAsync(request.PromoCode);
                return Ok(new { isValid = true, promotion = promotion });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation du code promo {PromoCode}", request.PromoCode);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpPost("promotions/calculate-discount")]
        public async Task<IActionResult> CalculateDiscount([FromBody] CalculateDiscountRequest request)
        {
            try
            {
                var discount = await _promotionService.CalculateDiscountAsync(
                    request.PromotionId, 
                    request.ProductId, 
                    request.ProductVariantId, 
                    request.Quantity, 
                    request.UnitPrice);

                return Ok(new { discount = discount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul de la réduction pour la promotion {PromotionId}", request.PromotionId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpPost("promotions/apply")]
        public async Task<IActionResult> ApplyPromotion([FromBody] ApplyPromotionRequest request)
        {
            try
            {
                var canApply = await _promotionService.CanApplyPromotionAsync(
                    request.PromotionId, 
                    request.ProductId, 
                    request.ProductVariantId, 
                    request.Quantity, 
                    request.TotalAmount);

                if (!canApply)
                {
                    return BadRequest(new { success = false, message = "Cette promotion ne peut pas être appliquée" });
                }

                var discount = await _promotionService.CalculateDiscountAsync(
                    request.PromotionId, 
                    request.ProductId, 
                    request.ProductVariantId, 
                    request.Quantity, 
                    request.UnitPrice);

                var usage = await _promotionService.ApplyPromotionAsync(
                    request.PromotionId, 
                    request.SaleId, 
                    discount, 
                    request.PromoCode, 
                    User.Identity?.Name);

                return Ok(new { success = true, discount = discount, usage = usage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'application de la promotion {PromotionId}", request.PromotionId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        #endregion

        #region Statistiques

        [HttpGet("promotions/statistics")]
        public async Task<IActionResult> GetPromotionStatistics()
        {
            try
            {
                var stats = await _promotionService.GetPromotionUsageStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques des promotions");
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpGet("promotions/{promotionId}/statistics")]
        public async Task<IActionResult> GetPromotionStatistics(int promotionId)
        {
            try
            {
                var stats = await _promotionService.GetPromotionStatisticsAsync(promotionId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques de la promotion {PromotionId}", promotionId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        #endregion
    }

    // Classes de requête
    public class CreatePromotionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PromotionType Type { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? MinimumPurchaseAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public int? BuyQuantity { get; set; }
        public int? GetQuantity { get; set; }
        public string? PromoCode { get; set; }
        public bool RequirePromoCode { get; set; }
        public bool ApplyToAllProducts { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int>? ProductIds { get; set; }
    }

    public class ValidatePromoCodeRequest
    {
        public string PromoCode { get; set; } = string.Empty;
    }

    public class CalculateDiscountRequest
    {
        public int PromotionId { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class ApplyPromotionRequest
    {
        public int PromotionId { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PromoCode { get; set; }
    }
}