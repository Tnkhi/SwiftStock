using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public decimal TotalRevenue { get; set; }
        public int TotalSales { get; set; }
        public decimal AverageSale { get; set; }
        public decimal GrossMargin { get; set; }
        public IList<DailySaleReport> DailySales { get; set; } = new List<DailySaleReport>();
        public IList<Product> Products { get; set; } = new List<Product>();
        public IList<Product> LowStockProducts { get; set; } = new List<Product>();
        public IList<Product> TopMarginProducts { get; set; } = new List<Product>();

        public async Task OnGetAsync(DateTime? dateFrom, DateTime? dateTo)
        {
            try
            {
                // Définir les dates par défaut si non fournies
                dateFrom ??= DateTime.Today.AddDays(-30);
                dateTo ??= DateTime.Today.AddDays(1); // +1 pour inclure toute la journée

                // Calculer les statistiques générales
                await CalculateGeneralStatistics(dateFrom.Value, dateTo.Value);

                // Calculer les ventes quotidiennes
                await CalculateDailySales(dateFrom.Value, dateTo.Value);

                // Charger les données des produits
                await LoadProductData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des rapports");
            }
        }

        private async Task CalculateGeneralStatistics(DateTime dateFrom, DateTime dateTo)
        {
            var salesQuery = _context.Sales
                .Where(s => s.SaleDate >= dateFrom && s.SaleDate < dateTo && s.Status == SaleStatus.Completed);

            TotalSales = await salesQuery.CountAsync();
            TotalRevenue = await salesQuery.SumAsync(s => s.TotalAmount);
            AverageSale = TotalSales > 0 ? TotalRevenue / TotalSales : 0;

            // Calculer la marge brute (approximation)
            var salesWithItems = await salesQuery
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .ToListAsync();

            var totalCost = salesWithItems
                .SelectMany(s => s.SaleItems)
                .Sum(si => si.Product.PurchasePrice * si.Quantity);

            GrossMargin = TotalRevenue - totalCost;
        }

        private async Task CalculateDailySales(DateTime dateFrom, DateTime dateTo)
        {
            var dailySales = await _context.Sales
                .Where(s => s.SaleDate >= dateFrom && s.SaleDate < dateTo && s.Status == SaleStatus.Completed)
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new DailySaleReport
                {
                    Date = g.Key,
                    SalesCount = g.Count(),
                    TotalAmount = g.Sum(s => s.TotalAmount),
                    AverageAmount = g.Average(s => s.TotalAmount)
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            // Remplir les jours manquants avec des valeurs nulles
            var allDays = new List<DailySaleReport>();
            for (var date = dateFrom.Date; date < dateTo.Date; date = date.AddDays(1))
            {
                var existingSale = dailySales.FirstOrDefault(d => d.Date.Date == date.Date);
                allDays.Add(existingSale ?? new DailySaleReport
                {
                    Date = date,
                    SalesCount = 0,
                    TotalAmount = 0,
                    AverageAmount = 0
                });
            }

            DailySales = allDays;
        }

        private async Task LoadProductData()
        {
            // Charger tous les produits avec leurs mouvements de stock
            Products = await _context.Products
                .Include(p => p.StockMovements)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            // Produits en stock bas
            LowStockProducts = Products
                .Where(p => p.IsLowStock)
                .OrderBy(p => p.CurrentStock)
                .Take(10)
                .ToList();

            // Top produits par marge
            TopMarginProducts = Products
                .Where(p => p.ProfitMargin > 0)
                .OrderByDescending(p => p.ProfitMargin)
                .Take(10)
                .ToList();
        }
    }

    public class DailySaleReport
    {
        public DateTime Date { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
    }
}
