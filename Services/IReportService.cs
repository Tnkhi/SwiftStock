using SwiftStock.Models;

namespace SwiftStock.Services
{
    public interface IReportService
    {
        // Rapports de ventes
        Task<object> GetSalesReportAsync(DateTime startDate, DateTime endDate, string? groupBy = null);
        Task<object> GetSalesByProductReportAsync(DateTime startDate, DateTime endDate);
        Task<object> GetSalesByCategoryReportAsync(DateTime startDate, DateTime endDate);
        Task<object> GetSalesByUserReportAsync(DateTime startDate, DateTime endDate);
        Task<object> GetSalesByPaymentMethodReportAsync(DateTime startDate, DateTime endDate);

        // Rapports de stock
        Task<object> GetStockReportAsync();
        Task<object> GetLowStockReportAsync();
        Task<object> GetStockMovementReportAsync(DateTime startDate, DateTime endDate);
        Task<object> GetStockValueReportAsync();

        // Rapports de performance
        Task<object> GetTopProductsReportAsync(DateTime startDate, DateTime endDate, int topCount = 10);
        Task<object> GetWorstProductsReportAsync(DateTime startDate, DateTime endDate, int topCount = 10);
        Task<object> GetProductPerformanceReportAsync(int productId, DateTime startDate, DateTime endDate);
        Task<object> GetCategoryPerformanceReportAsync(int categoryId, DateTime startDate, DateTime endDate);

        // Rapports d'achats
        Task<object> GetPurchaseReportAsync(DateTime startDate, DateTime endDate);
        Task<object> GetSupplierReportAsync(DateTime startDate, DateTime endDate);
        Task<object> GetPurchaseOrderStatusReportAsync();

        // Rapports financiers
        Task<object> GetProfitLossReportAsync(DateTime startDate, DateTime endDate);
        Task<object> GetMarginReportAsync(DateTime startDate, DateTime endDate);
        Task<object> GetCashFlowReportAsync(DateTime startDate, DateTime endDate);

        // Rapports de clients
        Task<object> GetCustomerReportAsync(DateTime startDate, DateTime endDate);
        Task<object> GetTopCustomersReportAsync(DateTime startDate, DateTime endDate, int topCount = 10);
        Task<object> GetCustomerLoyaltyReportAsync();

        // Rapports de promotions
        Task<object> GetPromotionReportAsync(DateTime startDate, DateTime endDate);
        Task<object> GetPromotionEffectivenessReportAsync(int promotionId);

        // Rapports de performance système
        Task<object> GetSystemPerformanceReportAsync();
        Task<object> GetUserActivityReportAsync(DateTime startDate, DateTime endDate);
        Task<object> GetAuditReportAsync(DateTime startDate, DateTime endDate);

        // Export de rapports
        Task<byte[]> ExportReportToExcelAsync(string reportType, DateTime startDate, DateTime endDate, object parameters);
        Task<byte[]> ExportReportToPdfAsync(string reportType, DateTime startDate, DateTime endDate, object parameters);
    }
}

