using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;
using SwiftStock.Services;
using System.ComponentModel.DataAnnotations;

namespace SwiftStock.Pages.Products.Categories
{
    [Authorize(Roles = "Admin,Manager,StockManager")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ApplicationDbContext context, IProductService productService, ILogger<CreateModel> logger)
        {
            _context = context;
            _productService = productService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public IList<ProductCategory> ParentCategories { get; set; } = new List<ProductCategory>();

        public class InputModel
        {
            [Required(ErrorMessage = "Le nom de la catégorie est requis")]
            [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
            [Display(Name = "Nom de la catégorie")]
            public string Name { get; set; } = string.Empty;

            [StringLength(50, ErrorMessage = "Le code ne peut pas dépasser 50 caractères")]
            [Display(Name = "Code")]
            public string? Code { get; set; }

            [StringLength(200, ErrorMessage = "La description ne peut pas dépasser 200 caractères")]
            [Display(Name = "Description")]
            public string? Description { get; set; }

            [Display(Name = "Catégorie parente")]
            public int? ParentCategoryId { get; set; }

            [Display(Name = "Image")]
            public IFormFile? ImageFile { get; set; }

            [Display(Name = "Catégorie active")]
            public bool IsActive { get; set; } = true;
        }

        public async Task OnGetAsync(int? parentId = null)
        {
            // Charger les catégories parentes disponibles
            ParentCategories = (await _productService.GetCategoriesAsync()).ToList();

            // Si un parentId est fourni, le pré-sélectionner
            if (parentId.HasValue)
            {
                Input.ParentCategoryId = parentId.Value;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadParentCategoriesAsync();
                return Page();
            }

            try
            {
                // Vérifier l'unicité du code si fourni
                if (!string.IsNullOrEmpty(Input.Code))
                {
                    var existingCategory = await _context.ProductCategories
                        .FirstOrDefaultAsync(pc => pc.Code == Input.Code);
                    
                    if (existingCategory != null)
                    {
                        ModelState.AddModelError("Input.Code", "Ce code de catégorie existe déjà.");
                        await LoadParentCategoriesAsync();
                        return Page();
                    }
                }

                // Vérifier que la catégorie parente existe si spécifiée
                if (Input.ParentCategoryId.HasValue)
                {
                    var parentCategory = await _context.ProductCategories
                        .FirstOrDefaultAsync(pc => pc.Id == Input.ParentCategoryId.Value);
                    
                    if (parentCategory == null)
                    {
                        ModelState.AddModelError("Input.ParentCategoryId", "La catégorie parente sélectionnée n'existe pas.");
                        await LoadParentCategoriesAsync();
                        return Page();
                    }
                }

                var category = new ProductCategory
                {
                    Name = Input.Name,
                    Code = Input.Code,
                    Description = Input.Description,
                    ParentCategoryId = Input.ParentCategoryId,
                    IsActive = Input.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = User.Identity?.Name
                };

                // Traitement de l'image si fournie
                if (Input.ImageFile != null && Input.ImageFile.Length > 0)
                {
                    var imageUrl = await SaveImageAsync(Input.ImageFile);
                    category.ImageUrl = imageUrl;
                }

                var createdCategory = await _productService.CreateCategoryAsync(category);

                _logger.LogInformation("Catégorie créée avec succès: {CategoryName} (ID: {CategoryId})", 
                    createdCategory.Name, createdCategory.Id);

                TempData["SuccessMessage"] = $"La catégorie '{createdCategory.Name}' a été créée avec succès.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la catégorie: {CategoryName}", Input.Name);
                ModelState.AddModelError(string.Empty, "Une erreur est survenue lors de la création de la catégorie.");
            }

            await LoadParentCategoriesAsync();
            return Page();
        }

        private async Task LoadParentCategoriesAsync()
        {
            ParentCategories = (await _productService.GetCategoriesAsync()).ToList();
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            // Créer le dossier wwwroot/images/categories s'il n'existe pas
            var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories");
            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }

            // Générer un nom de fichier unique
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(imagesPath, fileName);

            // Sauvegarder le fichier
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/categories/{fileName}";
        }
    }
}
