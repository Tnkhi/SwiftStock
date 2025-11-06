using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Models;

namespace SwiftStock.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Créer les rôles par défaut
            string[] roles = { "Admin", "Manager", "Cashier", "StockManager" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Créer les rôles personnalisés
            await CreateCustomRolesAsync(context);

            // Créer l'emplacement par défaut
            await CreateDefaultLocationAsync(context);

            // Créer les paramètres de l'entreprise
            await CreateCompanySettingsAsync(context);

            // Créer les catégories par défaut
            await CreateDefaultCategoriesAsync(context);

            // Créer l'utilisateur admin par défaut
            var adminEmail = "admin@swiftstock.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "SwiftStock",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Créer l'utilisateur caissier par défaut
            var cashierEmail = "cashier@swiftstock.com";
            var cashierUser = await userManager.FindByEmailAsync(cashierEmail);

            if (cashierUser == null)
            {
                cashierUser = new User
                {
                    UserName = cashierEmail,
                    Email = cashierEmail,
                    FirstName = "Caissier",
                    LastName = "Test",
                    Role = "Cashier",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(cashierUser, "Cashier123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(cashierUser, "Cashier");
                }
            }
        }

        private static async Task CreateCustomRolesAsync(ApplicationDbContext context)
        {
            if (!await context.UserRoles.AnyAsync())
            {
                var roles = new List<UserRole>
                {
                    new UserRole
                    {
                        Name = "Administrateur",
                        Description = "Accès complet à toutes les fonctionnalités",
                        CanManageUsers = true,
                        CanManageProducts = true,
                        CanManageStock = true,
                        CanProcessSales = true,
                        CanManagePurchases = true,
                        CanViewReports = true,
                        CanManageSettings = true,
                        CanVoidSales = true,
                        CanApplyDiscounts = true,
                        CanManageInventory = true,
                        CanExportData = true,
                        IsSystemRole = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new UserRole
                    {
                        Name = "Manager",
                        Description = "Gestion des utilisateurs et rapports",
                        CanManageUsers = true,
                        CanManageProducts = true,
                        CanManageStock = true,
                        CanProcessSales = true,
                        CanManagePurchases = true,
                        CanViewReports = true,
                        CanManageSettings = false,
                        CanVoidSales = true,
                        CanApplyDiscounts = true,
                        CanManageInventory = true,
                        CanExportData = true,
                        IsSystemRole = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new UserRole
                    {
                        Name = "Caissier",
                        Description = "Point de vente et consultation des ventes",
                        CanManageUsers = false,
                        CanManageProducts = false,
                        CanManageStock = false,
                        CanProcessSales = true,
                        CanManagePurchases = false,
                        CanViewReports = true,
                        CanManageSettings = false,
                        CanVoidSales = false,
                        CanApplyDiscounts = true,
                        CanManageInventory = false,
                        CanExportData = false,
                        IsSystemRole = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new UserRole
                    {
                        Name = "Gestionnaire Stock",
                        Description = "Gestion des produits et inventaire",
                        CanManageUsers = false,
                        CanManageProducts = true,
                        CanManageStock = true,
                        CanProcessSales = false,
                        CanManagePurchases = true,
                        CanViewReports = true,
                        CanManageSettings = false,
                        CanVoidSales = false,
                        CanApplyDiscounts = false,
                        CanManageInventory = true,
                        CanExportData = true,
                        IsSystemRole = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.UserRoles.AddRange(roles);
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateDefaultLocationAsync(ApplicationDbContext context)
        {
            if (!await context.Locations.AnyAsync())
            {
                var location = new Location
                {
                    Name = "Magasin Principal",
                    Code = "MAIN",
                    Address = "123 Avenue de la République",
                    City = "Abidjan",
                    PostalCode = "00225",
                    Country = "Côte d'Ivoire",
                    Phone = "+225 20 30 40 50",
                    Email = "contact@swiftstock.com",
                    Description = "Magasin principal de l'entreprise",
                    IsActive = true,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Locations.Add(location);
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateCompanySettingsAsync(ApplicationDbContext context)
        {
            if (!await context.CompanySettings.AnyAsync())
            {
                var settings = new CompanySettings
                {
                    CompanyName = "SwiftStock",
                    LegalName = "SwiftStock SARL",
                    TaxNumber = "CI-ABJ-2024-A-12345",
                    Address = "123 Avenue de la République",
                    City = "Abidjan",
                    PostalCode = "00225",
                    Country = "Côte d'Ivoire",
                    Phone = "+225 20 30 40 50",
                    Email = "contact@swiftstock.com",
                    Website = "https://swiftstock.com",
                    Currency = "FCFA",
                    CurrencySymbol = "F",
                    DefaultTaxRate = 18.00m,
                    TaxName = "TVA",
                    ReceiptHeader = "Merci pour votre achat !",
                    ReceiptFooter = "Rendez-vous bientôt !",
                    PrintReceipt = true,
                    PrintInvoice = false,
                    EnableLowStockAlerts = true,
                    DefaultLowStockThreshold = 10,
                    RequireCustomerInfo = false,
                    EnableDiscounts = true,
                    MaxDiscountPercentage = 50.00m,
                    RequirePasswordForVoid = true,
                    EnableAuditLog = true,
                    SessionTimeoutMinutes = 480,
                    AutoBackup = true,
                    BackupFrequencyDays = 7,
                    CreatedAt = DateTime.UtcNow
                };

                context.CompanySettings.Add(settings);
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateDefaultCategoriesAsync(ApplicationDbContext context)
        {
            if (!await context.ProductCategories.AnyAsync())
            {
                var categories = new List<ProductCategory>
                {
                    new ProductCategory
                    {
                        Name = "Alimentation",
                        Code = "FOOD",
                        Description = "Produits alimentaires et boissons",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductCategory
                    {
                        Name = "Électronique",
                        Code = "ELEC",
                        Description = "Appareils électroniques et accessoires",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductCategory
                    {
                        Name = "Vêtements",
                        Code = "CLOTH",
                        Description = "Vêtements et accessoires de mode",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductCategory
                    {
                        Name = "Maison & Jardin",
                        Code = "HOME",
                        Description = "Articles pour la maison et le jardin",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductCategory
                    {
                        Name = "Santé & Beauté",
                        Code = "HEALTH",
                        Description = "Produits de santé et de beauté",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductCategory
                    {
                        Name = "Sports & Loisirs",
                        Code = "SPORT",
                        Description = "Équipements sportifs et articles de loisirs",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.ProductCategories.AddRange(categories);
                await context.SaveChangesAsync();

                // Créer quelques sous-catégories
                var alimentation = categories.First(c => c.Code == "FOOD");
                var electronique = categories.First(c => c.Code == "ELEC");

                var subCategories = new List<ProductCategory>
                {
                    new ProductCategory
                    {
                        Name = "Boissons",
                        Code = "DRINKS",
                        Description = "Boissons non alcoolisées",
                        ParentCategoryId = alimentation.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductCategory
                    {
                        Name = "Produits laitiers",
                        Code = "DAIRY",
                        Description = "Lait, fromage, yaourts",
                        ParentCategoryId = alimentation.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductCategory
                    {
                        Name = "Smartphones",
                        Code = "PHONE",
                        Description = "Téléphones portables et accessoires",
                        ParentCategoryId = electronique.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductCategory
                    {
                        Name = "Ordinateurs",
                        Code = "COMP",
                        Description = "Ordinateurs portables et de bureau",
                        ParentCategoryId = electronique.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.ProductCategories.AddRange(subCategories);
                await context.SaveChangesAsync();
            }
        }
    }
}

