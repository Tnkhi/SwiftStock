using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Pages.POS.Promotions
{
    [Authorize(Roles = "Admin,Manager")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Promotion> Promotions { get; set; } = new List<Promotion>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TypeFilter { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Promotions
                .Include(p => p.CreatedBy)
                .Include(p => p.PromotionUsages)
                .AsQueryable();

            // Filtrage par recherche
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(p => 
                    p.Name.Contains(SearchTerm) ||
                    p.Description.Contains(SearchTerm) ||
                    p.PromoCode.Contains(SearchTerm));
            }

            // Filtrage par statut
            if (!string.IsNullOrEmpty(StatusFilter) && Enum.TryParse<PromotionStatus>(StatusFilter, out var status))
            {
                query = query.Where(p => p.Status == status);
            }

            // Filtrage par type
            if (!string.IsNullOrEmpty(TypeFilter) && Enum.TryParse<PromotionType>(TypeFilter, out var type))
            {
                query = query.Where(p => p.Type == type);
            }

            Promotions = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}

