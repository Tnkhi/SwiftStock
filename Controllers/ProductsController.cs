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
    [Authorize(Roles = "Admin,Manager,StockManager")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;
        private readonly IAuditService _auditService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            ApplicationDbContext context, 
            IProductService productService,
            IAuditService auditService,
            ILogger<ProductsController> logger)
        {
            _context = context;
            _productService = productService;
            _auditService = auditService;
            _logger = logger;
        }

        #region Gestion des catégories

        [HttpPost("categories/{categoryId}/toggle-status")]
        public async Task<IActionResult> ToggleCategoryStatus(int categoryId, [FromQuery] bool isActive)
        {
            try
            {
                var category = await _context.ProductCategories.FindAsync(categoryId);
                if (category == null)
                {
                    return NotFound("Catégorie non trouvée");
                }

                var oldStatus = category.IsActive;
                category.IsActive = isActive;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Enregistrer l'audit
                await _auditService.LogAsync(
                    AuditAction.Update,
                    "ProductCategory",
                    categoryId.ToString(),
                    category.Name,
                    $"Statut changé de {(oldStatus ? "Actif" : "Inactif")} à {(isActive ? "Actif" : "Inactif")}",
                    oldStatus.ToString(),
                    isActive.ToString(),
                    User.Identity?.Name,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );

                _logger.LogInformation("Statut de la catégorie {CategoryId} changé à {Status}", categoryId, isActive);
                return Ok(new { success = true, message = "Statut mis à jour avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de statut de la catégorie {CategoryId}", categoryId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpGet("categories/{categoryId}/products")]
        public async Task<IActionResult> GetCategoryProducts(int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var products = await _context.Products
                    .Where(p => p.CategoryId == categoryId && p.IsActive)
                    .Include(p => p.Category)
                    .OrderBy(p => p.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var totalCount = await _context.Products
                    .CountAsync(p => p.CategoryId == categoryId && p.IsActive);

                return Ok(new
                {
                    products = products,
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des produits de la catégorie {CategoryId}", categoryId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        #endregion

        #region Gestion des variantes

        [HttpPost("variants")]
        public async Task<IActionResult> CreateVariant([FromBody] CreateVariantRequest request)
        {
            try
            {
                var variant = new ProductVariant
                {
                    ProductId = request.ProductId,
                    Name = request.Name,
                    AttributeType = request.AttributeType,
                    SKU = request.SKU,
                    Barcode = request.Barcode,
                    PurchasePrice = request.PurchasePrice,
                    SalePrice = request.SalePrice,
                    CurrentStock = request.CurrentStock,
                    MinStockLevel = request.MinStockLevel,
                    MaxStockLevel = request.MaxStockLevel,
                    Description = request.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = User.Identity?.Name
                };

                var createdVariant = await _productService.CreateVariantAsync(variant);

                // Enregistrer l'audit
                await _auditService.LogAsync(
                    AuditAction.Create,
                    "ProductVariant",
                    createdVariant.Id.ToString(),
                    $"{createdVariant.Name} (Produit ID: {createdVariant.ProductId})",
                    "Variante de produit créée",
                    null,
                    $"Nom: {createdVariant.Name}, SKU: {createdVariant.SKU}",
                    User.Identity?.Name,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );

                return Ok(createdVariant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la variante pour le produit {ProductId}", request.ProductId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpGet("variants/{variantId}")]
        public async Task<IActionResult> GetVariant(int variantId)
        {
            try
            {
                var variant = await _productService.GetVariantAsync(variantId);
                if (variant == null)
                {
                    return NotFound("Variante non trouvée");
                }

                return Ok(variant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la variante {VariantId}", variantId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpGet("products/{productId}/variants")]
        public async Task<IActionResult> GetProductVariants(int productId)
        {
            try
            {
                var variants = await _productService.GetProductVariantsAsync(productId);
                return Ok(variants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des variantes du produit {ProductId}", productId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        #endregion

        #region Gestion des ajustements de stock

        [HttpPost("adjustments")]
        public async Task<IActionResult> CreateStockAdjustment([FromBody] CreateAdjustmentRequest request)
        {
            try
            {
                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                {
                    return NotFound("Produit non trouvé");
                }

                var adjustment = new StockAdjustment
                {
                    ProductId = request.ProductId,
                    ProductVariantId = request.ProductVariantId,
                    Type = request.Type,
                    QuantityDifference = request.QuantityDifference,
                    PreviousStock = product.CurrentStock,
                    NewStock = product.CurrentStock + request.QuantityDifference,
                    Reason = request.Reason,
                    Notes = request.Notes,
                    Reference = request.Reference,
                    AdjustmentDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = User.Identity?.Name ?? ""
                };

                var createdAdjustment = await _productService.CreateAdjustmentAsync(adjustment);

                // Enregistrer l'audit
                await _auditService.LogAsync(
                    AuditAction.Create,
                    "StockAdjustment",
                    createdAdjustment.Id.ToString(),
                    $"Ajustement pour {product.Name}",
                    $"Ajustement de stock créé: {request.QuantityDifference} unités",
                    product.CurrentStock.ToString(),
                    adjustment.NewStock.ToString(),
                    User.Identity?.Name,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );

                return Ok(createdAdjustment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'ajustement de stock pour le produit {ProductId}", request.ProductId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpGet("adjustments/pending")]
        public async Task<IActionResult> GetPendingAdjustments()
        {
            try
            {
                var adjustments = await _productService.GetPendingAdjustmentsAsync();
                return Ok(adjustments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des ajustements en attente");
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpPost("adjustments/{adjustmentId}/approve")]
        public async Task<IActionResult> ApproveAdjustment(int adjustmentId)
        {
            try
            {
                var success = await _productService.ApproveAdjustmentAsync(adjustmentId, User.Identity?.Name ?? "");
                if (!success)
                {
                    return NotFound("Ajustement non trouvé ou déjà traité");
                }

                return Ok(new { success = true, message = "Ajustement approuvé avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'approbation de l'ajustement {AdjustmentId}", adjustmentId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        #endregion

        #region Codes-barres

        [HttpGet("barcodes/generate")]
        public async Task<IActionResult> GenerateBarcode()
        {
            try
            {
                var barcode = await _productService.GenerateBarcodeAsync();
                return Ok(new { barcode = barcode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du code-barres");
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpGet("barcodes/{barcode}/validate")]
        public async Task<IActionResult> ValidateBarcode(string barcode)
        {
            try
            {
                var isValid = await _productService.ValidateBarcodeAsync(barcode);
                return Ok(new { isValid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation du code-barres {Barcode}", barcode);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpGet("barcodes/{barcode}/product")]
        public async Task<IActionResult> GetProductByBarcode(string barcode)
        {
            try
            {
                var product = await _productService.GetProductByBarcodeAsync(barcode);
                var variant = await _productService.GetVariantByBarcodeAsync(barcode);

                if (product == null && variant == null)
                {
                    return NotFound("Aucun produit trouvé avec ce code-barres");
                }

                return Ok(new { product = product, variant = variant });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche du produit par code-barres {Barcode}", barcode);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        #endregion

        #region Statistiques

        [HttpGet("statistics")]
        public async Task<IActionResult> GetProductStatistics()
        {
            try
            {
                var stats = await _productService.GetInventoryStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques des produits");
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpGet("products/{productId}/statistics")]
        public async Task<IActionResult> GetProductStatistics(int productId)
        {
            try
            {
                var stats = await _productService.GetProductStatisticsAsync(productId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques du produit {ProductId}", productId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        #endregion
    }

    // Classes de requête
    public class CreateVariantRequest
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? AttributeType { get; set; }
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }
        public string? Description { get; set; }
    }

    public class CreateAdjustmentRequest
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public AdjustmentType Type { get; set; }
        public int QuantityDifference { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public string? Reference { get; set; }
    }
}

