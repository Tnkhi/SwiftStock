using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;
using System.ComponentModel.DataAnnotations;

namespace SwiftStock.Pages.Admin.Users
{
    [Authorize(Roles = "Admin,Manager")]
    public class CreateModel : BasePageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(UserManager<User> userManager, ApplicationDbContext context, ILogger<CreateModel> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public IList<Location> Locations { get; set; } = new List<Location>();

        public class InputModel
        {
            [Required(ErrorMessage = "Le prénom est requis")]
            [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
            [Display(Name = "Prénom")]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Le nom est requis")]
            [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
            [Display(Name = "Nom")]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "L'email est requis")]
            [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Le mot de passe est requis")]
            [StringLength(100, ErrorMessage = "Le mot de passe doit contenir au moins {2} caractères", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mot de passe")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmer le mot de passe")]
            [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Le rôle est requis")]
            [Display(Name = "Rôle")]
            public string Role { get; set; } = string.Empty;

            [Display(Name = "Téléphone")]
            [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Adresse")]
            [StringLength(500, ErrorMessage = "L'adresse ne peut pas dépasser 500 caractères")]
            public string? Address { get; set; }

            [Display(Name = "Ville")]
            [StringLength(50, ErrorMessage = "La ville ne peut pas dépasser 50 caractères")]
            public string? City { get; set; }

            [Display(Name = "Pays")]
            [StringLength(50, ErrorMessage = "Le pays ne peut pas dépasser 50 caractères")]
            public string? Country { get; set; }

            [Display(Name = "Notes")]
            [StringLength(500, ErrorMessage = "Les notes ne peuvent pas dépasser 500 caractères")]
            public string? Notes { get; set; }

            [Display(Name = "Emplacement par défaut")]
            public int? DefaultLocationId { get; set; }

            [Display(Name = "Changer le mot de passe à la prochaine connexion")]
            public bool MustChangePassword { get; set; } = true;
        }

        public async Task OnGetAsync()
        {
            Locations = await _context.Locations
                .Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadLocationsAsync();
                return Page();
            }

            // Vérifier si l'email existe déjà
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Un utilisateur avec cet email existe déjà.");
                await LoadLocationsAsync();
                return Page();
            }

            try
            {
                var user = new User
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Role = Input.Role,
                    PhoneNumber = Input.PhoneNumber,
                    Address = Input.Address,
                    City = Input.City,
                    Country = Input.Country,
                    Notes = Input.Notes,
                    DefaultLocationId = Input.DefaultLocationId,
                    MustChangePassword = Input.MustChangePassword,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Utilisateur créé avec succès: {Email}", Input.Email);
                    TempData["SuccessMessage"] = $"L'utilisateur {Input.FirstName} {Input.LastName} a été créé avec succès.";
                    return RedirectToPage("./Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'utilisateur: {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, "Une erreur est survenue lors de la création de l'utilisateur.");
            }

            await LoadLocationsAsync();
            return Page();
        }

        private async Task LoadLocationsAsync()
        {
            Locations = await _context.Locations
                .Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .ToListAsync();
        }
    }
}

