using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Pages.Admin.Users
{
    [Authorize(Roles = "Admin,Manager")]
    public class DetailsModel : BasePageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public DetailsModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public User? UserModel { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalAmount { get; set; }
        public IList<Sale> RecentSales { get; set; } = new List<Sale>();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            UserModel = await _userManager.Users
                .Include(u => u.DefaultLocation)
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (UserModel == null)
            {
                return NotFound();
            }

            // Calculer les statistiques
            var salesQuery = _context.Sales.Where(s => s.CashierId == id);
            
            TotalSales = await salesQuery.CountAsync();
            TotalAmount = await salesQuery.SumAsync(s => s.TotalAmount);

            // Récupérer les ventes récentes
            RecentSales = await salesQuery
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .ToListAsync();

            return Page();
        }
    }
}
