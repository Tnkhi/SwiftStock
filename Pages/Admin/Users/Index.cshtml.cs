using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Pages.Admin.Users
{
    [Authorize(Roles = "Admin,Manager")]
    public class IndexModel : BasePageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IList<User> Users { get; set; } = new List<User>();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? RoleFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        
        public int TotalPages { get; set; }
        public int TotalUsers { get; set; }
        public int PageSize { get; set; } = 20;

        public async Task OnGetAsync()
        {
            var query = _userManager.Users
                .Include(u => u.UserRole)
                .Include(u => u.DefaultLocation)
                .AsQueryable();

            // Filtrage par recherche
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(u => 
                    u.FirstName.Contains(SearchTerm) ||
                    u.LastName.Contains(SearchTerm) ||
                    u.Email.Contains(SearchTerm));
            }

            // Filtrage par rôle
            if (!string.IsNullOrEmpty(RoleFilter))
            {
                query = query.Where(u => u.Role == RoleFilter);
            }

            // Filtrage par statut
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                var isActive = StatusFilter == "true";
                query = query.Where(u => u.IsActive == isActive);
            }

            // Compter le total
            TotalUsers = await query.CountAsync();
            TotalPages = (int)Math.Ceiling((double)TotalUsers / PageSize);

            // Pagination
            Users = await query
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }

    public class BasePageModel : PageModel
    {
        // Classe de base pour les pages d'administration
        // Peut contenir des méthodes communes
    }
}
