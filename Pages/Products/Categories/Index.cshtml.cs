using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Pages.Products.Categories
{
    [Authorize(Roles = "Admin,Manager,StockManager")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<ProductCategory> Categories { get; set; } = new List<ProductCategory>();
        public IList<ProductCategory> ParentCategories { get; set; } = new List<ProductCategory>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ParentCategoryId { get; set; }

        public async Task OnGetAsync()
        {
            // Charger les catégories parentes pour le filtre
            ParentCategories = await _context.ProductCategories
                .Where(pc => pc.ParentCategoryId == null && pc.IsActive)
                .OrderBy(pc => pc.Name)
                .ToListAsync();

            var query = _context.ProductCategories
                .Include(pc => pc.ParentCategory)
                .Include(pc => pc.SubCategories)
                .Include(pc => pc.Products)
                .AsQueryable();

            // Filtrage par recherche
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(pc => 
                    pc.Name.Contains(SearchTerm) ||
                    pc.Description.Contains(SearchTerm) ||
                    pc.Code.Contains(SearchTerm));
            }

            // Filtrage par catégorie parente
            if (ParentCategoryId.HasValue)
            {
                if (ParentCategoryId == 0)
                {
                    // Catégories principales uniquement
                    query = query.Where(pc => pc.ParentCategoryId == null);
                }
                else
                {
                    // Sous-catégories d'une catégorie parente spécifique
                    query = query.Where(pc => pc.ParentCategoryId == ParentCategoryId);
                }
            }

            Categories = await query
                .OrderBy(pc => pc.ParentCategoryId)
                .ThenBy(pc => pc.Name)
                .ToListAsync();
        }
    }
}

