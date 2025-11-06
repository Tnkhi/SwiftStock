using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Rapports de ventes

        public async Task<object> GetSalesReportAsync(DateTime startDate, DateTime endDate, string? groupBy = null)
        {
            var sales = await _context.Sales
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate && s.Status == SaleStatus.Completed)
                .Include(s => s.Cashier)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .ToListAsync();

            var totalSales = sales.Count;
            var totalRevenue = sales.Sum(s => s.TotalAmount);
            var averageSale = totalSales > 0 ? totalRevenue / totalSales : 0;

            var salesByDay = sales.GroupBy(s => s.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(s => s.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            var salesByPaymentMethod = sales.GroupBy(s => s.PaymentMethod)
                .Select(g => new
                {
                    PaymentMethod = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(s => s.TotalAmount),
                    Percentage = totalRevenue > 0 ? (g.Sum(s => s.TotalAmount) / totalRevenue) * 100 : 0
                })
                .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                Summary = new
                {
                    TotalSales = totalSales,
                    TotalRevenue = totalRevenue,
                    AverageSale = averageSale
                },
                SalesByDay = salesByDay,
                SalesByPaymentMethod = salesByPaymentMethod
            };
        }

        public async Task<object> GetSalesByProductReportAsync(DateTime startDate, DateTime endDate)
        {
            var salesItems = await _context.SaleItems
                .Where(si => si.Sale.SaleDate >= startDate && 
                           si.Sale.SaleDate <= endDate && 
                           si.Sale.Status == SaleStatus.Completed)
                .Include(si => si.Product)
                .Include(si => si.Sale)
                .ToListAsync();

            var productSales = salesItems.GroupBy(si => si.Product)
                .Select(g => new
                {
                    ProductId = g.Key.Id,
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(si => si.Quantity),
                    TotalRevenue = g.Sum(si => si.TotalPrice),
                    AveragePrice = g.Average(si => si.UnitPrice),
                    SaleCount = g.Count()
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                ProductSales = productSales
            };
        }

        public async Task<object> GetSalesByCategoryReportAsync(DateTime startDate, DateTime endDate)
        {
            var salesItems = await _context.SaleItems
                .Where(si => si.Sale.SaleDate >= startDate && 
                           si.Sale.SaleDate <= endDate && 
                           si.Sale.Status == SaleStatus.Completed)
                .Include(si => si.Product)
                    .ThenInclude(p => p.Category)
                .Include(si => si.Sale)
                .ToListAsync();

            var categorySales = salesItems.GroupBy(si => si.Product.Category)
                .Select(g => new
                {
                    CategoryId = g.Key?.Id,
                    CategoryName = g.Key?.Name ?? "Sans catégorie",
                    TotalQuantity = g.Sum(si => si.Quantity),
                    TotalRevenue = g.Sum(si => si.TotalPrice),
                    ProductCount = g.Select(si => si.ProductId).Distinct().Count()
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                CategorySales = categorySales
            };
        }

        public async Task<object> GetSalesByUserReportAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _context.Sales
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate && s.Status == SaleStatus.Completed)
                .Include(s => s.Cashier)
                .ToListAsync();

            var userSales = sales.GroupBy(s => s.Cashier)
                .Select(g => new
                {
                    UserId = g.Key?.Id,
                    UserName = g.Key != null ? $"{g.Key.FirstName} {g.Key.LastName}" : "Inconnu",
                    TotalSales = g.Count(),
                    TotalRevenue = g.Sum(s => s.TotalAmount),
                    AverageSale = g.Average(s => s.TotalAmount)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                UserSales = userSales
            };
        }

        public async Task<object> GetSalesByPaymentMethodReportAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _context.Sales
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate && s.Status == SaleStatus.Completed)
                .ToListAsync();

            var paymentMethodSales = sales.GroupBy(s => s.PaymentMethod)
                .Select(g => new
                {
                    PaymentMethod = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(s => s.TotalAmount),
                    Percentage = sales.Sum(s => s.TotalAmount) > 0 ? 
                        (g.Sum(s => s.TotalAmount) / sales.Sum(s => s.TotalAmount)) * 100 : 0
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                PaymentMethodSales = paymentMethodSales
            };
        }

        #endregion

        #region Rapports de stock

        public async Task<object> GetStockReportAsync()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .ToListAsync();

            var totalProducts = products.Count;
            var totalStock = products.Sum(p => p.CurrentStock);
            var totalValue = products.Sum(p => p.CurrentStock * p.PurchasePrice);
            var averageStock = totalProducts > 0 ? totalStock / totalProducts : 0;

            var stockByCategory = products.GroupBy(p => p.Category)
                .Select(g => new
                {
                    CategoryId = g.Key?.Id,
                    CategoryName = g.Key?.Name ?? "Sans catégorie",
                    ProductCount = g.Count(),
                    TotalStock = g.Sum(p => p.CurrentStock),
                    TotalValue = g.Sum(p => p.CurrentStock * p.PurchasePrice)
                })
                .OrderByDescending(x => x.TotalValue)
                .ToList();

            return new
            {
                Summary = new
                {
                    TotalProducts = totalProducts,
                    TotalStock = totalStock,
                    TotalValue = totalValue,
                    AverageStock = averageStock
                },
                StockByCategory = stockByCategory
            };
        }

        public async Task<object> GetLowStockReportAsync()
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.IsActive && p.CurrentStock <= p.MinStockLevel)
                .Include(p => p.Category)
                .OrderBy(p => p.CurrentStock)
                .ToListAsync();

            var outOfStockProducts = lowStockProducts.Where(p => p.CurrentStock <= 0).ToList();
            var lowStockCount = lowStockProducts.Count;
            var outOfStockCount = outOfStockProducts.Count;

            return new
            {
                LowStockProducts = lowStockProducts.Select(p => new
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CurrentStock = p.CurrentStock,
                    MinStockLevel = p.MinStockLevel,
                    CategoryName = p.Category?.Name ?? "Sans catégorie",
                    IsOutOfStock = p.CurrentStock <= 0
                }),
                Summary = new
                {
                    LowStockCount = lowStockCount,
                    OutOfStockCount = outOfStockCount,
                    TotalValue = lowStockProducts.Sum(p => p.CurrentStock * p.PurchasePrice)
                }
            };
        }

        public async Task<object> GetStockMovementReportAsync(DateTime startDate, DateTime endDate)
        {
            var movements = await _context.StockMovements
                .Where(sm => sm.CreatedAt >= startDate && sm.CreatedAt <= endDate)
                .Include(sm => sm.Product)
                .Include(sm => sm.CreatedBy)
                .ToListAsync();

            var movementsByType = movements.GroupBy(sm => sm.Type)
                .Select(g => new
                {
                    MovementType = g.Key,
                    Count = g.Count(),
                    TotalQuantity = g.Sum(sm => sm.Quantity)
                })
                .ToList();

            var movementsByProduct = movements.GroupBy(sm => sm.Product)
                .Select(g => new
                {
                    ProductId = g.Key.Id,
                    ProductName = g.Key.Name,
                    TotalMovements = g.Count(),
                    NetQuantity = g.Sum(sm => sm.Quantity)
                })
                .OrderByDescending(x => Math.Abs(x.NetQuantity))
                .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                MovementsByType = movementsByType,
                MovementsByProduct = movementsByProduct
            };
        }

        public async Task<object> GetStockValueReportAsync()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .ToListAsync();

            var totalValue = products.Sum(p => p.CurrentStock * p.PurchasePrice);
            var averageValue = products.Count > 0 ? totalValue / products.Count : 0;

            var valueByCategory = products.GroupBy(p => p.Category)
                .Select(g => new
                {
                    CategoryId = g.Key?.Id,
                    CategoryName = g.Key?.Name ?? "Sans catégorie",
                    ProductCount = g.Count(),
                    TotalValue = g.Sum(p => p.CurrentStock * p.PurchasePrice),
                    Percentage = totalValue > 0 ? (g.Sum(p => p.CurrentStock * p.PurchasePrice) / totalValue) * 100 : 0
                })
                .OrderByDescending(x => x.TotalValue)
                .ToList();

            return new
            {
                Summary = new
                {
                    TotalValue = totalValue,
                    AverageValue = averageValue,
                    ProductCount = products.Count
                },
                ValueByCategory = valueByCategory
            };
        }

        #endregion

        #region Rapports de performance

        public async Task<object> GetTopProductsReportAsync(DateTime startDate, DateTime endDate, int topCount = 10)
        {
            var salesItems = await _context.SaleItems
                .Where(si => si.Sale.SaleDate >= startDate && 
                           si.Sale.SaleDate <= endDate && 
                           si.Sale.Status == SaleStatus.Completed)
                .Include(si => si.Product)
                .Include(si => si.Sale)
                .ToListAsync();

            var topProducts = salesItems.GroupBy(si => si.Product)
                .Select(g => new
                {
                    ProductId = g.Key.Id,
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(si => si.Quantity),
                    TotalRevenue = g.Sum(si => si.TotalPrice),
                    SaleCount = g.Count(),
                    AveragePrice = g.Average(si => si.UnitPrice)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(topCount)
                .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                TopProducts = topProducts
            };
        }

        public async Task<object> GetWorstProductsReportAsync(DateTime startDate, DateTime endDate, int topCount = 10)
        {
            var salesItems = await _context.SaleItems
                .Where(si => si.Sale.SaleDate >= startDate && 
                           si.Sale.SaleDate <= endDate && 
                           si.Sale.Status == SaleStatus.Completed)
                .Include(si => si.Product)
                .Include(si => si.Sale)
                .ToListAsync();

            var worstProducts = salesItems.GroupBy(si => si.Product)
                .Select(g => new
                {
                    ProductId = g.Key.Id,
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(si => si.Quantity),
                    TotalRevenue = g.Sum(si => si.TotalPrice),
                    SaleCount = g.Count(),
                    AveragePrice = g.Average(si => si.UnitPrice)
                })
                .OrderBy(x => x.TotalRevenue)
                .Take(topCount)
                .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                WorstProducts = worstProducts
            };
        }

        public async Task<object> GetProductPerformanceReportAsync(int productId, DateTime startDate, DateTime endDate)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return new { };

            var salesItems = await _context.SaleItems
                .Where(si => si.ProductId == productId && 
                           si.Sale.SaleDate >= startDate && 
                           si.Sale.SaleDate <= endDate && 
                           si.Sale.Status == SaleStatus.Completed)
                .Include(si => si.Sale)
                .ToListAsync();

            var totalQuantity = salesItems.Sum(si => si.Quantity);
            var totalRevenue = salesItems.Sum(si => si.TotalPrice);
            var saleCount = salesItems.Count;
            var averagePrice = saleCount > 0 ? totalRevenue / totalQuantity : 0;

            var salesByDay = salesItems.GroupBy(si => si.Sale.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Quantity = g.Sum(si => si.Quantity),
                    Revenue = g.Sum(si => si.TotalPrice)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return new
            {
                Product = new
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    CategoryName = product.Category?.Name ?? "Sans catégorie",
                    CurrentStock = product.CurrentStock,
                    PurchasePrice = product.PurchasePrice,
                    SalePrice = product.SalePrice
                },
                Period = new { StartDate = startDate, EndDate = endDate },
                Performance = new
                {
                    TotalQuantity = totalQuantity,
                    TotalRevenue = totalRevenue,
                    SaleCount = saleCount,
                    AveragePrice = averagePrice
                },
                SalesByDay = salesByDay
            };
        }

        public async Task<object> GetCategoryPerformanceReportAsync(int categoryId, DateTime startDate, DateTime endDate)
        {
            var category = await _context.ProductCategories.FindAsync(categoryId);
            if (category == null) return new { };

            var salesItems = await _context.SaleItems
                .Where(si => si.Product.CategoryId == categoryId && 
                           si.Sale.SaleDate >= startDate && 
                           si.Sale.SaleDate <= endDate && 
                           si.Sale.Status == SaleStatus.Completed)
                .Include(si => si.Product)
                .Include(si => si.Sale)
                .ToListAsync();

            var totalQuantity = salesItems.Sum(si => si.Quantity);
            var totalRevenue = salesItems.Sum(si => si.TotalPrice);
            var productCount = salesItems.Select(si => si.ProductId).Distinct().Count();
            var saleCount = salesItems.Count;

            var topProducts = salesItems.GroupBy(si => si.Product)
                .Select(g => new
                {
                    ProductId = g.Key.Id,
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(si => si.Quantity),
                    TotalRevenue = g.Sum(si => si.TotalPrice)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(5)
                .ToList();

            return new
            {
                Category = new
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    Description = category.Description
                },
                Period = new { StartDate = startDate, EndDate = endDate },
                Performance = new
                {
                    TotalQuantity = totalQuantity,
                    TotalRevenue = totalRevenue,
                    ProductCount = productCount,
                    SaleCount = saleCount
                },
                TopProducts = topProducts
            };
        }

        #endregion

        #region Rapports d'achats

        public async Task<object> GetPurchaseReportAsync(DateTime startDate, DateTime endDate)
        {
            var purchases = await _context.Purchases
                .Where(p => p.OrderDate >= startDate && p.OrderDate <= endDate)
                .Include(p => p.Supplier)
                .Include(p => p.CreatedBy)
                .ToListAsync();

            var totalPurchases = purchases.Count;
            var totalAmount = purchases.Sum(p => p.TotalAmount);
            var averagePurchase = totalPurchases > 0 ? totalAmount / totalPurchases : 0;

            var purchasesBySupplier = purchases.GroupBy(p => p.Supplier)
                .Select(g => new
                {
                    SupplierId = g.Key.Id,
                    SupplierName = g.Key.Name,
                    PurchaseCount = g.Count(),
                    TotalAmount = g.Sum(p => p.TotalAmount),
                    AverageAmount = g.Average(p => p.TotalAmount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                Summary = new
                {
                    TotalPurchases = totalPurchases,
                    TotalAmount = totalAmount,
                    AveragePurchase = averagePurchase
                },
                PurchasesBySupplier = purchasesBySupplier
            };
        }

        public async Task<object> GetSupplierReportAsync(DateTime startDate, DateTime endDate)
        {
            var suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .Include(s => s.Purchases.Where(p => p.OrderDate >= startDate && p.OrderDate <= endDate))
                .ToListAsync();

            var supplierStats = suppliers.Select(s => new
            {
                SupplierId = s.Id,
                SupplierName = s.Name,
                ContactPerson = s.ContactPerson,
                PhoneNumber = s.Phone,
                Email = s.Email,
                PurchaseCount = s.Purchases.Count,
                TotalAmount = s.Purchases.Sum(p => p.TotalAmount),
                LastPurchaseDate = s.Purchases.Any() ? s.Purchases.Max(p => p.OrderDate) : (DateTime?)null
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                Suppliers = supplierStats
            };
        }

        public async Task<object> GetPurchaseOrderStatusReportAsync()
        {
            var orders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .ToListAsync();

            var statusCounts = orders.GroupBy(po => po.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(po => po.TotalAmount)
                })
                .ToList();

            var overdueOrders = orders.Where(po => po.ExpectedDeliveryDate.HasValue && 
                                                 po.ExpectedDeliveryDate < DateTime.Today && 
                                                 po.Status != PurchaseOrderStatus.Received &&
                                                 po.Status != PurchaseOrderStatus.Cancelled)
                .Select(po => new
                {
                    OrderId = po.Id,
                    OrderNumber = po.OrderNumber,
                    SupplierName = po.Supplier.Name,
                    ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                    DaysOverdue = (DateTime.Today - po.ExpectedDeliveryDate.Value).Days,
                    TotalAmount = po.TotalAmount
                })
                .OrderBy(po => po.ExpectedDeliveryDate)
                .ToList();

            return new
            {
                StatusCounts = statusCounts,
                OverdueOrders = overdueOrders,
                Summary = new
                {
                    TotalOrders = orders.Count,
                    TotalValue = orders.Sum(po => po.TotalAmount),
                    OverdueCount = overdueOrders.Count
                }
            };
        }

        #endregion

        #region Rapports financiers

        public async Task<object> GetProfitLossReportAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _context.Sales
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate && s.Status == SaleStatus.Completed)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .ToListAsync();

            var purchases = await _context.Purchases
                .Where(p => p.OrderDate >= startDate && p.OrderDate <= endDate)
                .ToListAsync();

            var totalRevenue = sales.Sum(s => s.TotalAmount);
            var totalCostOfGoodsSold = sales.SelectMany(s => s.SaleItems)
                .Sum(si => si.Quantity * si.Product.PurchasePrice);
            var totalPurchases = purchases.Sum(p => p.TotalAmount);

            var grossProfit = totalRevenue - totalCostOfGoodsSold;
            var grossProfitMargin = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0;

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                Revenue = new
                {
                    TotalRevenue = totalRevenue,
                    TotalCostOfGoodsSold = totalCostOfGoodsSold,
                    GrossProfit = grossProfit,
                    GrossProfitMargin = grossProfitMargin
                },
                Purchases = new
                {
                    TotalPurchases = totalPurchases,
                    PurchaseCount = purchases.Count
                }
            };
        }

        public async Task<object> GetMarginReportAsync(DateTime startDate, DateTime endDate)
        {
            var salesItems = await _context.SaleItems
                .Where(si => si.Sale.SaleDate >= startDate && 
                           si.Sale.SaleDate <= endDate && 
                           si.Sale.Status == SaleStatus.Completed)
                .Include(si => si.Product)
                .Include(si => si.Sale)
                .ToListAsync();

            var marginByProduct = salesItems.GroupBy(si => si.Product)
                .Select(g => new
                {
                    ProductId = g.Key.Id,
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(si => si.Quantity),
                    TotalRevenue = g.Sum(si => si.TotalPrice),
                    TotalCost = g.Sum(si => si.Quantity * g.Key.PurchasePrice),
                    TotalMargin = g.Sum(si => si.TotalPrice) - g.Sum(si => si.Quantity * g.Key.PurchasePrice),
                    MarginPercentage = g.Sum(si => si.TotalPrice) > 0 ? 
                        ((g.Sum(si => si.TotalPrice) - g.Sum(si => si.Quantity * g.Key.PurchasePrice)) / g.Sum(si => si.TotalPrice)) * 100 : 0
                })
                .OrderByDescending(x => x.TotalMargin)
                .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                MarginByProduct = marginByProduct
            };
        }

        public async Task<object> GetCashFlowReportAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _context.Sales
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate && s.Status == SaleStatus.Completed)
                .ToListAsync();

            var purchases = await _context.Purchases
                .Where(p => p.OrderDate >= startDate && p.OrderDate <= endDate)
                .ToListAsync();

            var cashInflow = sales.Where(s => s.PaymentMethod == PaymentMethod.Cash)
                .Sum(s => s.TotalAmount);

            var cardInflow = sales.Where(s => s.PaymentMethod == PaymentMethod.Card)
                .Sum(s => s.TotalAmount);

            var cashOutflow = purchases.Sum(p => p.TotalAmount);

            var netCashFlow = cashInflow - cashOutflow;

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                CashFlow = new
                {
                    CashInflow = cashInflow,
                    CardInflow = cardInflow,
                    TotalInflow = cashInflow + cardInflow,
                    CashOutflow = cashOutflow,
                    NetCashFlow = netCashFlow
                }
            };
        }

        #endregion

        #region Rapports de clients

        public async Task<object> GetCustomerReportAsync(DateTime startDate, DateTime endDate)
        {
            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .Include(c => c.Sales.Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate))
                .ToListAsync();

            var customerStats = customers.Select(c => new
            {
                CustomerId = c.Id,
                CustomerName = $"{c.FirstName} {c.LastName}",
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                PurchaseCount = c.Sales.Count,
                TotalAmount = c.Sales.Sum(s => s.TotalAmount),
                LastPurchaseDate = c.Sales.Any() ? c.Sales.Max(s => s.SaleDate) : (DateTime?)null
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                Customers = customerStats
            };
        }

        public async Task<object> GetTopCustomersReportAsync(DateTime startDate, DateTime endDate, int topCount = 10)
        {
            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .Include(c => c.Sales.Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate))
                .ToListAsync();

            var topCustomers = customers
                .Where(c => c.Sales.Any())
                .Select(c => new
                {
                    CustomerId = c.Id,
                    CustomerName = $"{c.FirstName} {c.LastName}",
                    Email = c.Email,
                    PurchaseCount = c.Sales.Count,
                    TotalAmount = c.Sales.Sum(s => s.TotalAmount),
                    AveragePurchase = c.Sales.Average(s => s.TotalAmount),
                    LastPurchaseDate = c.Sales.Max(s => s.SaleDate)
                })
                .OrderByDescending(x => x.TotalAmount)
                .Take(topCount)
                .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                TopCustomers = topCustomers
            };
        }

        public async Task<object> GetCustomerLoyaltyReportAsync()
        {
            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .Include(c => c.Sales)
                .ToListAsync();

            var loyaltyStats = customers.Select(c => new
            {
                CustomerId = c.Id,
                CustomerName = $"{c.FirstName} {c.LastName}",
                TotalPurchases = c.Sales.Count,
                TotalAmount = c.Sales.Sum(s => s.TotalAmount),
                FirstPurchaseDate = c.Sales.Any() ? c.Sales.Min(s => s.SaleDate) : (DateTime?)null,
                LastPurchaseDate = c.Sales.Any() ? c.Sales.Max(s => s.SaleDate) : (DateTime?)null,
                DaysSinceLastPurchase = c.Sales.Any() ? (DateTime.Today - c.Sales.Max(s => s.SaleDate)).Days : (int?)null
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

            return new
            {
                LoyaltyStats = loyaltyStats
            };
        }

        #endregion

        #region Rapports de promotions

        public async Task<object> GetPromotionReportAsync(DateTime startDate, DateTime endDate)
        {
            var promotions = await _context.Promotions
                .Where(p => p.IsActive)
                .Include(p => p.PromotionUsages.Where(pu => pu.UsedAt >= startDate && pu.UsedAt <= endDate))
                .ToListAsync();

            var promotionStats = promotions.Select(p => new
            {
                PromotionId = p.Id,
                PromotionName = p.Name,
                Type = p.Type,
                Status = p.Status,
                UsageCount = p.PromotionUsages.Count,
                TotalDiscount = p.PromotionUsages.Sum(pu => pu.DiscountAmount),
                AverageDiscount = p.PromotionUsages.Any() ? p.PromotionUsages.Average(pu => pu.DiscountAmount) : 0
            })
            .OrderByDescending(x => x.UsageCount)
            .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                Promotions = promotionStats
            };
        }

        public async Task<object> GetPromotionEffectivenessReportAsync(int promotionId)
        {
            var promotion = await _context.Promotions
                .Include(p => p.PromotionUsages)
                    .ThenInclude(pu => pu.Sale)
                .FirstOrDefaultAsync(p => p.Id == promotionId);

            if (promotion == null) return new { };

            var totalUsage = promotion.PromotionUsages.Count;
            var totalDiscount = promotion.PromotionUsages.Sum(pu => pu.DiscountAmount);
            var averageDiscount = totalUsage > 0 ? totalDiscount / totalUsage : 0;

            var usageByDay = promotion.PromotionUsages.GroupBy(pu => pu.UsedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    UsageCount = g.Count(),
                    TotalDiscount = g.Sum(pu => pu.DiscountAmount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return new
            {
                Promotion = new
                {
                    PromotionId = promotion.Id,
                    PromotionName = promotion.Name,
                    Type = promotion.Type,
                    Status = promotion.Status
                },
                Effectiveness = new
                {
                    TotalUsage = totalUsage,
                    TotalDiscount = totalDiscount,
                    AverageDiscount = averageDiscount
                },
                UsageByDay = usageByDay
            };
        }

        #endregion

        #region Rapports de performance système

        public async Task<object> GetSystemPerformanceReportAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
            var totalCustomers = await _context.Customers.CountAsync(c => c.IsActive);
            var totalSuppliers = await _context.Suppliers.CountAsync(s => s.IsActive);

            var todaySales = await _context.Sales
                .Where(s => s.SaleDate.Date == DateTime.Today && s.Status == SaleStatus.Completed)
                .CountAsync();

            var todayRevenue = await _context.Sales
                .Where(s => s.SaleDate.Date == DateTime.Today && s.Status == SaleStatus.Completed)
                .SumAsync(s => s.TotalAmount);

            return new
            {
                Users = new
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers
                },
                Products = new
                {
                    TotalProducts = totalProducts
                },
                Customers = new
                {
                    TotalCustomers = totalCustomers
                },
                Suppliers = new
                {
                    TotalSuppliers = totalSuppliers
                },
                Today = new
                {
                    Sales = todaySales,
                    Revenue = todayRevenue
                }
            };
        }

        public async Task<object> GetUserActivityReportAsync(DateTime startDate, DateTime endDate)
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .Include(u => u.Sales.Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate))
                .ToListAsync();

            var userActivity = users.Select(u => new
            {
                UserId = u.Id,
                UserName = $"{u.FirstName} {u.LastName}",
                Role = u.Role,
                SalesCount = u.Sales.Count,
                TotalRevenue = u.Sales.Sum(s => s.TotalAmount),
                LastActivity = u.Sales.Any() ? u.Sales.Max(s => s.SaleDate) : (DateTime?)null
            })
            .OrderByDescending(x => x.SalesCount)
            .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                UserActivity = userActivity
            };
        }

        public async Task<object> GetAuditReportAsync(DateTime startDate, DateTime endDate)
        {
            var auditLogs = await _context.AuditLogs
                .Where(al => al.Timestamp >= startDate && al.Timestamp <= endDate)
                .Include(al => al.User)
                .ToListAsync();

            var logsByAction = auditLogs.GroupBy(al => al.Action)
                .Select(g => new
                {
                    Action = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            var logsByUser = auditLogs.GroupBy(al => al.User)
                .Select(g => new
                {
                    UserId = g.Key?.Id,
                    UserName = g.Key != null ? $"{g.Key.FirstName} {g.Key.LastName}" : "Système",
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                LogsByAction = logsByAction,
                LogsByUser = logsByUser,
                Summary = new
                {
                    TotalLogs = auditLogs.Count,
                    UniqueUsers = auditLogs.Select(al => al.UserId).Distinct().Count()
                }
            };
        }

        #endregion

        #region Export de rapports

        public Task<byte[]> ExportReportToExcelAsync(string reportType, DateTime startDate, DateTime endDate, object parameters)
        {
            // Implémentation de l'export Excel
            // Pour l'instant, retourner un tableau vide
            return Task.FromResult(new byte[0]);
        }

        public Task<byte[]> ExportReportToPdfAsync(string reportType, DateTime startDate, DateTime endDate, object parameters)
        {
            // Implémentation de l'export PDF
            // Pour l'instant, retourner un tableau vide
            return Task.FromResult(new byte[0]);
        }

        #endregion
    }
}
