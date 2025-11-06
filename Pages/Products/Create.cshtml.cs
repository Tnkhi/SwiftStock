using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ApplicationDbContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        [BindProperty]
        public int InitialStock { get; set; } = 0;

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Vérifier l'unicité du SKU si fourni
                if (!string.IsNullOrEmpty(Product.SKU))
                {
                    var existingProduct = await _context.Products
                        .FirstOrDefaultAsync(p => p.SKU == Product.SKU);
                    
                    if (existingProduct != null)
                    {
                        ModelState.AddModelError("Product.SKU", "Cette référence (SKU) existe déjà.");
                        return Page();
                    }
                }

                // Vérifier l'unicité du code-barres si fourni
                if (!string.IsNullOrEmpty(Product.Barcode))
                {
                    var existingProduct = await _context.Products
                        .FirstOrDefaultAsync(p => p.Barcode == Product.Barcode);
                    
                    if (existingProduct != null)
                    {
                        ModelState.AddModelError("Product.Barcode", "Ce code-barres existe déjà.");
                        return Page();
                    }
                }

                // Définir les valeurs par défaut
                Product.CreatedAt = DateTime.UtcNow;
                Product.UpdatedAt = DateTime.UtcNow;
                Product.IsActive = true;

                // Ajouter le produit à la base de données
                _context.Products.Add(Product);
                await _context.SaveChangesAsync();

                // Créer un mouvement de stock initial si spécifié
                if (InitialStock > 0)
                {
                    var stockMovement = new StockMovement
                    {
                        ProductId = Product.Id,
                        Type = MovementType.Adjustment,
                        Quantity = InitialStock,
                        Notes = "Stock initial",
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = User.Identity?.Name
                    };

                    _context.StockMovements.Add(stockMovement);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Produit créé avec succès: {ProductName} (ID: {ProductId})", Product.Name, Product.Id);

                TempData["SuccessMessage"] = $"Le produit '{Product.Name}' a été créé avec succès.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du produit: {ProductName}", Product.Name);
                ModelState.AddModelError("", "Une erreur est survenue lors de la création du produit. Veuillez réessayer.");
                return Page();
            }
        }
    }
}
