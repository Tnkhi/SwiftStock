using Microsoft.AspNetCore.Mvc;
using SwiftStock.Services;

namespace SwiftStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        #region Rapports de ventes

        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string? groupBy = null)
        {
            try
            {
                var report = await _reportService.GetSalesReportAsync(startDate, endDate, groupBy);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de ventes");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("sales/by-product")]
        public async Task<IActionResult> GetSalesByProductReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetSalesByProductReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de ventes par produit");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("sales/by-category")]
        public async Task<IActionResult> GetSalesByCategoryReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetSalesByCategoryReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de ventes par catégorie");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("sales/by-user")]
        public async Task<IActionResult> GetSalesByUserReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetSalesByUserReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de ventes par utilisateur");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("sales/by-payment-method")]
        public async Task<IActionResult> GetSalesByPaymentMethodReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetSalesByPaymentMethodReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de ventes par méthode de paiement");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        #endregion

        #region Rapports de stock

        [HttpGet("stock")]
        public async Task<IActionResult> GetStockReport()
        {
            try
            {
                var report = await _reportService.GetStockReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de stock");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("stock/low-stock")]
        public async Task<IActionResult> GetLowStockReport()
        {
            try
            {
                var report = await _reportService.GetLowStockReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de stock faible");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("stock/movements")]
        public async Task<IActionResult> GetStockMovementReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetStockMovementReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de mouvements de stock");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("stock/value")]
        public async Task<IActionResult> GetStockValueReport()
        {
            try
            {
                var report = await _reportService.GetStockValueReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de valeur de stock");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        #endregion

        #region Rapports de performance

        [HttpGet("performance/top-products")]
        public async Task<IActionResult> GetTopProductsReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int topCount = 10)
        {
            try
            {
                var report = await _reportService.GetTopProductsReportAsync(startDate, endDate, topCount);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport des meilleurs produits");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("performance/worst-products")]
        public async Task<IActionResult> GetWorstProductsReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int topCount = 10)
        {
            try
            {
                var report = await _reportService.GetWorstProductsReportAsync(startDate, endDate, topCount);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport des moins bons produits");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("performance/product/{productId}")]
        public async Task<IActionResult> GetProductPerformanceReport(int productId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetProductPerformanceReportAsync(productId, startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de performance du produit");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("performance/category/{categoryId}")]
        public async Task<IActionResult> GetCategoryPerformanceReport(int categoryId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetCategoryPerformanceReportAsync(categoryId, startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de performance de la catégorie");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        #endregion

        #region Rapports d'achats

        [HttpGet("purchases")]
        public async Task<IActionResult> GetPurchaseReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetPurchaseReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport d'achats");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("purchases/suppliers")]
        public async Task<IActionResult> GetSupplierReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetSupplierReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport des fournisseurs");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("purchases/orders/status")]
        public async Task<IActionResult> GetPurchaseOrderStatusReport()
        {
            try
            {
                var report = await _reportService.GetPurchaseOrderStatusReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de statut des commandes");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        #endregion

        #region Rapports financiers

        [HttpGet("financial/profit-loss")]
        public async Task<IActionResult> GetProfitLossReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetProfitLossReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de profit et perte");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("financial/margin")]
        public async Task<IActionResult> GetMarginReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetMarginReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de marge");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("financial/cash-flow")]
        public async Task<IActionResult> GetCashFlowReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetCashFlowReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de flux de trésorerie");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        #endregion

        #region Rapports de clients

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomerReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetCustomerReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport des clients");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("customers/top")]
        public async Task<IActionResult> GetTopCustomersReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int topCount = 10)
        {
            try
            {
                var report = await _reportService.GetTopCustomersReportAsync(startDate, endDate, topCount);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport des meilleurs clients");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("customers/loyalty")]
        public async Task<IActionResult> GetCustomerLoyaltyReport()
        {
            try
            {
                var report = await _reportService.GetCustomerLoyaltyReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de fidélité des clients");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        #endregion

        #region Rapports de promotions

        [HttpGet("promotions")]
        public async Task<IActionResult> GetPromotionReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetPromotionReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport des promotions");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("promotions/{promotionId}/effectiveness")]
        public async Task<IActionResult> GetPromotionEffectivenessReport(int promotionId)
        {
            try
            {
                var report = await _reportService.GetPromotionEffectivenessReportAsync(promotionId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport d'efficacité de la promotion");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        #endregion

        #region Rapports de performance système

        [HttpGet("system/performance")]
        public async Task<IActionResult> GetSystemPerformanceReport()
        {
            try
            {
                var report = await _reportService.GetSystemPerformanceReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de performance système");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("system/user-activity")]
        public async Task<IActionResult> GetUserActivityReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetUserActivityReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport d'activité des utilisateurs");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        [HttpGet("system/audit")]
        public async Task<IActionResult> GetAuditReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportService.GetAuditReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport d'audit");
                return StatusCode(500, "Erreur lors de la génération du rapport");
            }
        }

        #endregion

        #region Export de rapports

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportReportToExcel([FromQuery] string reportType, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] object parameters)
        {
            try
            {
                var fileBytes = await _reportService.ExportReportToExcelAsync(reportType, startDate, endDate, parameters);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"rapport_{reportType}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export Excel du rapport");
                return StatusCode(500, "Erreur lors de l'export du rapport");
            }
        }

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportReportToPdf([FromQuery] string reportType, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] object parameters)
        {
            try
            {
                var fileBytes = await _reportService.ExportReportToPdfAsync(reportType, startDate, endDate, parameters);
                return File(fileBytes, "application/pdf", $"rapport_{reportType}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export PDF du rapport");
                return StatusCode(500, "Erreur lors de l'export du rapport");
            }
        }

        #endregion
    }
}

