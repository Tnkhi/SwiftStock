using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(ApplicationDbContext context, ILogger<DetailsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Product? Product { get; set; }
        public IList<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Charger le produit avec ses mouvements de stock
                Product = await _context.Products
                    .Include(p => p.StockMovements)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (Product == null)
                {
                    return NotFound();
                }

                // Charger l'historique des mouvements de stock
                StockMovements = await _context.StockMovements
                    .Include(sm => sm.CreatedBy)
                    .Where(sm => sm.ProductId == id)
                    .OrderByDescending(sm => sm.CreatedAt)
                    .Take(50) // Limiter à 50 derniers mouvements
                    .ToListAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des détails du produit ID: {ProductId}", id);
                return NotFound();
            }
        }
    }
}
