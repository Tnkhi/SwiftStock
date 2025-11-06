using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Pages.Sales
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

        public IList<Sale> Sales { get; set; } = new List<Sale>();
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageSale { get; set; }
        public int TodaySales { get; set; }

        public async Task OnGetAsync(DateTime? dateFrom, DateTime? dateTo, string? paymentMethod)
        {
            try
            {
                // Définir les dates par défaut si non fournies
                dateFrom ??= DateTime.Today.AddDays(-7);
                dateTo ??= DateTime.Today.AddDays(1); // +1 pour inclure toute la journée

                var query = _context.Sales
                    .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                    .Include(s => s.Cashier)
                    .AsQueryable();

                // Appliquer les filtres
                query = query.Where(s => s.SaleDate >= dateFrom && s.SaleDate < dateTo);

                if (!string.IsNullOrEmpty(paymentMethod) && Enum.TryParse<PaymentMethod>(paymentMethod, out var method))
                {
                    query = query.Where(s => s.PaymentMethod == method);
                }

                // Charger les ventes
                Sales = await query
                    .OrderByDescending(s => s.SaleDate)
                    .Take(100) // Limiter à 100 dernières ventes
                    .ToListAsync();

                // Calculer les statistiques
                TotalSales = await query.CountAsync();
                TotalRevenue = await query.SumAsync(s => s.TotalAmount);
                AverageSale = TotalSales > 0 ? TotalRevenue / TotalSales : 0;

                // Ventes d'aujourd'hui
                var today = DateTime.Today;
                TodaySales = await _context.Sales
                    .Where(s => s.SaleDate.Date == today && s.Status == SaleStatus.Completed)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des ventes");
                Sales = new List<Sale>();
            }
        }
    }
}
