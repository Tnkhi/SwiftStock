using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Pages.Products
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
        public IList<string> Categories { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            try
            {
                // Charger tous les produits avec leurs mouvements de stock
                Products = await _context.Products
                    .Include(p => p.StockMovements)
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                // Récupérer les catégories uniques
                Categories = await _context.Products
                    .Where(p => p.IsActive && !string.IsNullOrEmpty(p.CategoryName))
                    .Select(p => p.CategoryName!)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des produits");
                Products = new List<Product>();
                Categories = new List<string>();
            }
        }
    }
}
