using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(ApplicationDbContext context, ILogger<PurchaseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Gestion des bons de commande

        public async Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder order)
        {
            if (string.IsNullOrEmpty(order.OrderNumber))
            {
                order.OrderNumber = await GenerateOrderNumberAsync();
            }

            _context.PurchaseOrders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<PurchaseOrder?> GetPurchaseOrderAsync(int orderId)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.CreatedBy)
                .Include(po => po.ApprovedBy)
                .Include(po => po.Items)
                    .ThenInclude(poi => poi.Product)
                .Include(po => po.Items)
                    .ThenInclude(poi => poi.ProductVariant)
                .Include(po => po.Receipts)
                .FirstOrDefaultAsync(po => po.Id == orderId);
        }

        public async Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersAsync()
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.CreatedBy)
                .Include(po => po.Items)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        public async Task<PurchaseOrder> UpdatePurchaseOrderAsync(PurchaseOrder order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            _context.PurchaseOrders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> ApprovePurchaseOrderAsync(int orderId, string approvedById)
        {
            var order = await _context.PurchaseOrders.FindAsync(orderId);
            if (order == null || order.Status != PurchaseOrderStatus.Draft) return false;

            order.Status = PurchaseOrderStatus.Confirmed;
            order.ApprovedAt = DateTime.UtcNow;
            order.ApprovedById = approvedById;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelPurchaseOrderAsync(int orderId)
        {
            var order = await _context.PurchaseOrders.FindAsync(orderId);
            if (order == null || order.Status == PurchaseOrderStatus.Cancelled) return false;

            order.Status = PurchaseOrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Gestion des items de commande

        public async Task<PurchaseOrderItem> AddItemToOrderAsync(PurchaseOrderItem item)
        {
            _context.PurchaseOrderItems.Add(item);
            await _context.SaveChangesAsync();

            // Recalculer le total de la commande
            await RecalculateOrderTotalAsync(item.PurchaseOrderId);

            return item;
        }

        public async Task<PurchaseOrderItem?> GetOrderItemAsync(int itemId)
        {
            return await _context.PurchaseOrderItems
                .Include(poi => poi.Product)
                .Include(poi => poi.ProductVariant)
                .Include(poi => poi.PurchaseOrder)
                .FirstOrDefaultAsync(poi => poi.Id == itemId);
        }

        public async Task<PurchaseOrderItem> UpdateOrderItemAsync(PurchaseOrderItem item)
        {
            _context.PurchaseOrderItems.Update(item);
            await _context.SaveChangesAsync();

            // Recalculer le total de la commande
            await RecalculateOrderTotalAsync(item.PurchaseOrderId);

            return item;
        }

        public async Task<bool> RemoveItemFromOrderAsync(int itemId)
        {
            var item = await _context.PurchaseOrderItems.FindAsync(itemId);
            if (item == null) return false;

            var orderId = item.PurchaseOrderId;
            _context.PurchaseOrderItems.Remove(item);
            await _context.SaveChangesAsync();

            // Recalculer le total de la commande
            await RecalculateOrderTotalAsync(orderId);

            return true;
        }

        #endregion

        #region Gestion des réceptions

        public async Task<PurchaseReceipt> CreateReceiptAsync(PurchaseReceipt receipt)
        {
            if (string.IsNullOrEmpty(receipt.ReceiptNumber))
            {
                receipt.ReceiptNumber = await GenerateReceiptNumberAsync();
            }

            _context.PurchaseReceipts.Add(receipt);
            await _context.SaveChangesAsync();
            return receipt;
        }

        public async Task<PurchaseReceipt?> GetReceiptAsync(int receiptId)
        {
            return await _context.PurchaseReceipts
                .Include(pr => pr.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(pr => pr.CreatedBy)
                .Include(pr => pr.VerifiedBy)
                .Include(pr => pr.Items)
                    .ThenInclude(pri => pri.Product)
                .Include(pr => pr.Items)
                    .ThenInclude(pri => pri.ProductVariant)
                .FirstOrDefaultAsync(pr => pr.Id == receiptId);
        }

        public async Task<IEnumerable<PurchaseReceipt>> GetOrderReceiptsAsync(int orderId)
        {
            return await _context.PurchaseReceipts
                .Where(pr => pr.PurchaseOrderId == orderId)
                .Include(pr => pr.CreatedBy)
                .Include(pr => pr.Items)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();
        }

        public async Task<PurchaseReceipt> UpdateReceiptAsync(PurchaseReceipt receipt)
        {
            receipt.UpdatedAt = DateTime.UtcNow;
            _context.PurchaseReceipts.Update(receipt);
            await _context.SaveChangesAsync();
            return receipt;
        }

        public async Task<bool> VerifyReceiptAsync(int receiptId, string verifiedById)
        {
            var receipt = await _context.PurchaseReceipts.FindAsync(receiptId);
            if (receipt == null) return false;

            receipt.Status = ReceiptStatus.Verified;
            receipt.VerifiedAt = DateTime.UtcNow;
            receipt.VerifiedById = verifiedById;
            receipt.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Gestion des items de réception

        public async Task<PurchaseReceiptItem> AddItemToReceiptAsync(PurchaseReceiptItem item)
        {
            _context.PurchaseReceiptItems.Add(item);
            await _context.SaveChangesAsync();

            // Recalculer le total de la réception
            await RecalculateReceiptTotalAsync(item.PurchaseReceiptId);

            return item;
        }

        public async Task<PurchaseReceiptItem?> GetReceiptItemAsync(int itemId)
        {
            return await _context.PurchaseReceiptItems
                .Include(pri => pri.Product)
                .Include(pri => pri.ProductVariant)
                .Include(pri => pri.PurchaseReceipt)
                .FirstOrDefaultAsync(pri => pri.Id == itemId);
        }

        public async Task<PurchaseReceiptItem> UpdateReceiptItemAsync(PurchaseReceiptItem item)
        {
            _context.PurchaseReceiptItems.Update(item);
            await _context.SaveChangesAsync();

            // Recalculer le total de la réception
            await RecalculateReceiptTotalAsync(item.PurchaseReceiptId);

            return item;
        }

        public async Task<bool> RemoveItemFromReceiptAsync(int itemId)
        {
            var item = await _context.PurchaseReceiptItems.FindAsync(itemId);
            if (item == null) return false;

            var receiptId = item.PurchaseReceiptId;
            _context.PurchaseReceiptItems.Remove(item);
            await _context.SaveChangesAsync();

            // Recalculer le total de la réception
            await RecalculateReceiptTotalAsync(receiptId);

            return true;
        }

        #endregion

        #region Workflow de réception

        public async Task<PurchaseReceipt> ProcessReceiptAsync(int receiptId)
        {
            var receipt = await _context.PurchaseReceipts
                .Include(pr => pr.Items)
                .FirstOrDefaultAsync(pr => pr.Id == receiptId);

            if (receipt == null) throw new ArgumentException("Réception non trouvée");

            receipt.Status = ReceiptStatus.Complete;
            receipt.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return receipt;
        }

        public async Task<bool> CompleteReceiptAsync(int receiptId)
        {
            var receipt = await _context.PurchaseReceipts
                .Include(pr => pr.PurchaseOrder)
                .FirstOrDefaultAsync(pr => pr.Id == receiptId);

            if (receipt == null) return false;

            // Mettre à jour le statut de la commande
            var order = receipt.PurchaseOrder;
            var totalReceived = await _context.PurchaseReceipts
                .Where(pr => pr.PurchaseOrderId == order.Id)
                .SumAsync(pr => pr.TotalAmount);

            if (totalReceived >= order.TotalAmount)
            {
                order.Status = PurchaseOrderStatus.Received;
            }
            else
            {
                order.Status = PurchaseOrderStatus.PartiallyReceived;
            }

            order.ReceivedAmount = totalReceived;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStockFromReceiptAsync(int receiptId)
        {
            var receipt = await _context.PurchaseReceipts
                .Include(pr => pr.Items)
                .FirstOrDefaultAsync(pr => pr.Id == receiptId);

            if (receipt == null) return false;

            foreach (var item in receipt.Items)
            {
                // Mettre à jour le stock du produit
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.CurrentStock += item.ReceivedQuantity;
                    product.UpdatedAt = DateTime.UtcNow;
                }

                // Créer un mouvement de stock
                var stockMovement = new StockMovement
                {
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    Type = MovementType.Purchase,
                    Quantity = item.ReceivedQuantity,
                    Notes = $"Réception - {receipt.ReceiptNumber}",
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = receipt.CreatedById
                };

                _context.StockMovements.Add(stockMovement);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Statistiques et rapports

        public async Task<object> GetPurchaseOrderStatisticsAsync()
        {
            var totalOrders = await _context.PurchaseOrders.CountAsync();
            var pendingOrders = await _context.PurchaseOrders
                .CountAsync(po => po.Status == PurchaseOrderStatus.Draft || po.Status == PurchaseOrderStatus.Sent);
            var overdueOrders = await _context.PurchaseOrders
                .CountAsync(po => po.ExpectedDeliveryDate.HasValue && 
                                 po.ExpectedDeliveryDate < DateTime.Today && 
                                 po.Status != PurchaseOrderStatus.Received);

            var totalValue = await _context.PurchaseOrders
                .SumAsync(po => po.TotalAmount);

            var thisMonthValue = await _context.PurchaseOrders
                .Where(po => po.CreatedAt.Month == DateTime.Today.Month && 
                           po.CreatedAt.Year == DateTime.Today.Year)
                .SumAsync(po => po.TotalAmount);

            return new
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                OverdueOrders = overdueOrders,
                TotalValue = totalValue,
                ThisMonthValue = thisMonthValue
            };
        }

        public async Task<object> GetSupplierStatisticsAsync(int supplierId)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null) return new { };

            var totalOrders = await _context.PurchaseOrders
                .CountAsync(po => po.SupplierId == supplierId);

            var totalValue = await _context.PurchaseOrders
                .Where(po => po.SupplierId == supplierId)
                .SumAsync(po => po.TotalAmount);

            var averageOrderValue = totalOrders > 0 ? totalValue / totalOrders : 0;

            var lastOrderDate = await _context.PurchaseOrders
                .Where(po => po.SupplierId == supplierId)
                .MaxAsync(po => (DateTime?)po.CreatedAt);

            return new
            {
                SupplierId = supplierId,
                SupplierName = supplier.Name,
                TotalOrders = totalOrders,
                TotalValue = totalValue,
                AverageOrderValue = averageOrderValue,
                LastOrderDate = lastOrderDate
            };
        }

        public async Task<IEnumerable<PurchaseOrder>> GetPendingOrdersAsync()
        {
            return await _context.PurchaseOrders
                .Where(po => po.Status == PurchaseOrderStatus.Draft || 
                           po.Status == PurchaseOrderStatus.Sent ||
                           po.Status == PurchaseOrderStatus.Confirmed)
                .Include(po => po.Supplier)
                .Include(po => po.Items)
                .OrderBy(po => po.ExpectedDeliveryDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseOrder>> GetOverdueOrdersAsync()
        {
            return await _context.PurchaseOrders
                .Where(po => po.ExpectedDeliveryDate.HasValue && 
                           po.ExpectedDeliveryDate < DateTime.Today && 
                           po.Status != PurchaseOrderStatus.Received &&
                           po.Status != PurchaseOrderStatus.Cancelled)
                .Include(po => po.Supplier)
                .Include(po => po.Items)
                .OrderBy(po => po.ExpectedDeliveryDate)
                .ToListAsync();
        }

        #endregion

        #region Génération de numéros

        public async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.Today;
            var prefix = $"PO{today:yyyyMM}";
            
            var lastOrder = await _context.PurchaseOrders
                .Where(po => po.OrderNumber.StartsWith(prefix))
                .OrderByDescending(po => po.OrderNumber)
                .FirstOrDefaultAsync();

            var sequence = 1;
            if (lastOrder != null)
            {
                var lastSequence = lastOrder.OrderNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out var lastNum))
                {
                    sequence = lastNum + 1;
                }
            }

            return $"{prefix}{sequence:D4}";
        }

        public async Task<string> GenerateReceiptNumberAsync()
        {
            var today = DateTime.Today;
            var prefix = $"PR{today:yyyyMM}";
            
            var lastReceipt = await _context.PurchaseReceipts
                .Where(pr => pr.ReceiptNumber.StartsWith(prefix))
                .OrderByDescending(pr => pr.ReceiptNumber)
                .FirstOrDefaultAsync();

            var sequence = 1;
            if (lastReceipt != null)
            {
                var lastSequence = lastReceipt.ReceiptNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out var lastNum))
                {
                    sequence = lastNum + 1;
                }
            }

            return $"{prefix}{sequence:D4}";
        }

        #endregion

        #region Méthodes privées

        private async Task RecalculateOrderTotalAsync(int orderId)
        {
            var order = await _context.PurchaseOrders.FindAsync(orderId);
            if (order == null) return;

            var items = await _context.PurchaseOrderItems
                .Where(poi => poi.PurchaseOrderId == orderId)
                .ToListAsync();

            order.SubTotal = items.Sum(i => i.TotalPrice);
            order.TaxAmount = items.Sum(i => i.TaxAmount);
            order.DiscountAmount = items.Sum(i => i.DiscountAmount);
            order.TotalAmount = order.SubTotal + order.TaxAmount - order.DiscountAmount;

            await _context.SaveChangesAsync();
        }

        private async Task RecalculateReceiptTotalAsync(int receiptId)
        {
            var receipt = await _context.PurchaseReceipts.FindAsync(receiptId);
            if (receipt == null) return;

            var items = await _context.PurchaseReceiptItems
                .Where(pri => pri.PurchaseReceiptId == receiptId)
                .ToListAsync();

            receipt.TotalAmount = items.Sum(i => i.TotalPrice);

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
