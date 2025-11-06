using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ApplicationDbContext _context;

    public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public decimal TodaySales { get; set; }
    public int PendingOrders { get; set; }
    public List<Product> LowStockProductsList { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            // Total des produits
            TotalProducts = await _context.Products.CountAsync(p => p.IsActive);

            // Produits en stock bas
            LowStockProductsList = await _context.Products
                .Where(p => p.IsActive && p.CurrentStock <= p.MinStockLevel)
                .OrderBy(p => p.CurrentStock)
                .Take(10)
                .ToListAsync();

            LowStockProducts = LowStockProductsList.Count;

            // Ventes d'aujourd'hui
            var today = DateTime.Today;
            TodaySales = await _context.Sales
                .Where(s => s.SaleDate.Date == today && s.Status == SaleStatus.Completed)
                .SumAsync(s => s.TotalAmount);

            // Commandes en attente
            PendingOrders = await _context.Purchases
                .CountAsync(p => p.Status == PurchaseStatus.Pending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement du tableau de bord");
            // Valeurs par défaut en cas d'erreur
            TotalProducts = 0;
            LowStockProducts = 0;
            TodaySales = 0;
            PendingOrders = 0;
        }
    }
}
