using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Pages.POS
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

        public IList<Product> Products { get; set; } = new List<Product>();

        public async Task OnGetAsync()
        {
            try
            {
                // Charger tous les produits actifs avec stock disponible
                Products = await _context.Products
                    .Include(p => p.StockMovements)
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des produits pour le POS");
                Products = new List<Product>();
            }
        }
    }
}
