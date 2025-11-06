using SwiftStock.Models;

namespace SwiftStock.Services
{
    public interface IPurchaseService
    {
        // Gestion des bons de commande
        Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder order);
        Task<PurchaseOrder?> GetPurchaseOrderAsync(int orderId);
        Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersAsync();
        Task<PurchaseOrder> UpdatePurchaseOrderAsync(PurchaseOrder order);
        Task<bool> ApprovePurchaseOrderAsync(int orderId, string approvedById);
        Task<bool> CancelPurchaseOrderAsync(int orderId);

        // Gestion des items de commande
        Task<PurchaseOrderItem> AddItemToOrderAsync(PurchaseOrderItem item);
        Task<PurchaseOrderItem?> GetOrderItemAsync(int itemId);
        Task<PurchaseOrderItem> UpdateOrderItemAsync(PurchaseOrderItem item);
        Task<bool> RemoveItemFromOrderAsync(int itemId);

        // Gestion des réceptions
        Task<PurchaseReceipt> CreateReceiptAsync(PurchaseReceipt receipt);
        Task<PurchaseReceipt?> GetReceiptAsync(int receiptId);
        Task<IEnumerable<PurchaseReceipt>> GetOrderReceiptsAsync(int orderId);
        Task<PurchaseReceipt> UpdateReceiptAsync(PurchaseReceipt receipt);
        Task<bool> VerifyReceiptAsync(int receiptId, string verifiedById);

        // Gestion des items de réception
        Task<PurchaseReceiptItem> AddItemToReceiptAsync(PurchaseReceiptItem item);
        Task<PurchaseReceiptItem?> GetReceiptItemAsync(int itemId);
        Task<PurchaseReceiptItem> UpdateReceiptItemAsync(PurchaseReceiptItem item);
        Task<bool> RemoveItemFromReceiptAsync(int itemId);

        // Workflow de réception
        Task<PurchaseReceipt> ProcessReceiptAsync(int receiptId);
        Task<bool> CompleteReceiptAsync(int receiptId);
        Task<bool> UpdateStockFromReceiptAsync(int receiptId);

        // Statistiques et rapports
        Task<object> GetPurchaseOrderStatisticsAsync();
        Task<object> GetSupplierStatisticsAsync(int supplierId);
        Task<IEnumerable<PurchaseOrder>> GetPendingOrdersAsync();
        Task<IEnumerable<PurchaseOrder>> GetOverdueOrdersAsync();

        // Génération de numéros
        Task<string> GenerateOrderNumberAsync();
        Task<string> GenerateReceiptNumberAsync();
    }
}

