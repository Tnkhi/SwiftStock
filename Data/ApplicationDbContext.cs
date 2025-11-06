using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Models;

namespace SwiftStock.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets pour nos entités
        public DbSet<Product> Products { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        
        // Nouveaux modèles d'administration
        public DbSet<Location> Locations { get; set; }
        public DbSet<CompanySettings> CompanySettings { get; set; }
        public new DbSet<UserRole> UserRoles { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        
        // Nouveaux modèles de produits avancés
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<StockAdjustment> StockAdjustments { get; set; }
        public DbSet<PhysicalInventory> PhysicalInventories { get; set; }
        public DbSet<PhysicalInventoryItem> PhysicalInventoryItems { get; set; }
        
        // Nouveaux modèles POS avancés
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionProduct> PromotionProducts { get; set; }
        public DbSet<PromotionUsage> PromotionUsages { get; set; }
        
        // Nouveaux modèles d'achats avancés
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<PurchaseReceipt> PurchaseReceipts { get; set; }
        public DbSet<PurchaseReceiptItem> PurchaseReceiptItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuration des relations et contraintes

            // Product
            builder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique()
                .HasFilter("SKU IS NOT NULL");

            builder.Entity<Product>()
                .HasIndex(p => p.Barcode)
                .IsUnique()
                .HasFilter("Barcode IS NOT NULL");

            // StockMovement
            builder.Entity<StockMovement>()
                .HasOne(sm => sm.Product)
                .WithMany(p => p.StockMovements)
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StockMovement>()
                .HasOne(sm => sm.CreatedBy)
                .WithMany(u => u.StockMovements)
                .HasForeignKey(sm => sm.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Sale
            builder.Entity<Sale>()
                .HasIndex(s => s.SaleNumber)
                .IsUnique();

            builder.Entity<Sale>()
                .HasOne(s => s.Cashier)
                .WithMany(u => u.Sales)
                .HasForeignKey(s => s.CashierId)
                .OnDelete(DeleteBehavior.SetNull);

            // SaleItem
            builder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany(p => p.SaleItems)
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Purchase
            builder.Entity<Purchase>()
                .HasIndex(p => p.PurchaseNumber)
                .IsUnique();

            builder.Entity<Purchase>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Purchases)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Purchase>()
                .HasOne(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            // PurchaseItem
            builder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Purchase)
                .WithMany(p => p.PurchaseItems)
                .HasForeignKey(pi => pi.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.PurchaseItems)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuration des types de données
            builder.Entity<Product>()
                .Property(p => p.PurchasePrice)
                .HasPrecision(18, 2);

            builder.Entity<Product>()
                .Property(p => p.SalePrice)
                .HasPrecision(18, 2);

            builder.Entity<Product>()
                .Property(p => p.TaxRate)
                .HasPrecision(5, 2);

            // Configuration des valeurs par défaut
            builder.Entity<Sale>()
                .Property(s => s.SaleNumber)
                .HasDefaultValueSql("'SALE-' || nextval('sale_number_seq')");

            builder.Entity<Purchase>()
                .Property(p => p.PurchaseNumber)
                .HasDefaultValueSql("'PUR-' || nextval('purchase_number_seq')");

            // Configuration des nouveaux modèles d'administration
            
            // Location
            builder.Entity<Location>()
                .HasIndex(l => l.Code)
                .IsUnique()
                .HasFilter("Code IS NOT NULL");

            builder.Entity<Location>()
                .HasOne(l => l.CreatedBy)
                .WithMany(u => u.CreatedLocations)
                .HasForeignKey(l => l.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            // CompanySettings (une seule instance)
            builder.Entity<CompanySettings>()
                .HasIndex(cs => cs.Id)
                .IsUnique();

            builder.Entity<CompanySettings>()
                .HasOne(cs => cs.UpdatedBy)
                .WithMany()
                .HasForeignKey(cs => cs.UpdatedById)
                .OnDelete(DeleteBehavior.SetNull);

            // UserRole
            builder.Entity<UserRole>()
                .HasIndex(ur => ur.Name)
                .IsUnique();

            builder.Entity<UserRole>()
                .HasOne(ur => ur.CreatedBy)
                .WithMany()
                .HasForeignKey(ur => ur.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            // User - relations avec les nouveaux modèles
            builder.Entity<User>()
                .HasOne(u => u.UserRole)
                .WithMany(ur => ur.Users)
                .HasForeignKey(u => u.UserRoleId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<User>()
                .HasOne(u => u.DefaultLocation)
                .WithMany()
                .HasForeignKey(u => u.DefaultLocationId)
                .OnDelete(DeleteBehavior.SetNull);

            // AuditLog
            builder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Notification
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasOne(n => n.CreatedBy)
                .WithMany(u => u.CreatedNotifications)
                .HasForeignKey(n => n.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuration des nouveaux modèles de produits avancés
            
            // ProductCategory
            builder.Entity<ProductCategory>()
                .HasIndex(pc => pc.Code)
                .IsUnique()
                .HasFilter("Code IS NOT NULL");

            builder.Entity<ProductCategory>()
                .HasOne(pc => pc.ParentCategory)
                .WithMany(pc => pc.SubCategories)
                .HasForeignKey(pc => pc.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductCategory>()
                .HasOne(pc => pc.CreatedBy)
                .WithMany()
                .HasForeignKey(pc => pc.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            // ProductVariant
            builder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductVariant>()
                .HasOne(pv => pv.CreatedBy)
                .WithMany()
                .HasForeignKey(pv => pv.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Product - relation avec Category
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(pc => pc.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // StockAdjustment
            builder.Entity<StockAdjustment>()
                .HasOne(sa => sa.Product)
                .WithMany(p => p.StockAdjustments)
                .HasForeignKey(sa => sa.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StockAdjustment>()
                .HasOne(sa => sa.ProductVariant)
                .WithMany()
                .HasForeignKey(sa => sa.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<StockAdjustment>()
                .HasOne(sa => sa.CreatedBy)
                .WithMany()
                .HasForeignKey(sa => sa.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StockAdjustment>()
                .HasOne(sa => sa.ApprovedBy)
                .WithMany()
                .HasForeignKey(sa => sa.ApprovedById)
                .OnDelete(DeleteBehavior.SetNull);

            // PhysicalInventory
            builder.Entity<PhysicalInventory>()
                .HasOne(pi => pi.CreatedBy)
                .WithMany()
                .HasForeignKey(pi => pi.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // PhysicalInventoryItem
            builder.Entity<PhysicalInventoryItem>()
                .HasOne(pii => pii.PhysicalInventory)
                .WithMany(pi => pi.Items)
                .HasForeignKey(pii => pii.PhysicalInventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PhysicalInventoryItem>()
                .HasOne(pii => pii.Product)
                .WithMany(p => p.PhysicalInventoryItems)
                .HasForeignKey(pii => pii.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PhysicalInventoryItem>()
                .HasOne(pii => pii.ProductVariant)
                .WithMany()
                .HasForeignKey(pii => pii.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<PhysicalInventoryItem>()
                .HasOne(pii => pii.CountedBy)
                .WithMany()
                .HasForeignKey(pii => pii.CountedById)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<PhysicalInventoryItem>()
                .HasOne(pii => pii.VerifiedBy)
                .WithMany()
                .HasForeignKey(pii => pii.VerifiedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Mise à jour des relations existantes pour inclure ProductVariant
            builder.Entity<StockMovement>()
                .HasOne(sm => sm.ProductVariant)
                .WithMany()
                .HasForeignKey(sm => sm.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<SaleItem>()
                .HasOne(si => si.ProductVariant)
                .WithMany()
                .HasForeignKey(si => si.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuration des nouveaux modèles POS avancés
            
            // Customer
            builder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique()
                .HasFilter("Email IS NOT NULL");

            builder.Entity<Customer>()
                .HasIndex(c => c.PhoneNumber)
                .IsUnique()
                .HasFilter("PhoneNumber IS NOT NULL");

            builder.Entity<Customer>()
                .HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Promotion
            builder.Entity<Promotion>()
                .HasIndex(p => p.PromoCode)
                .IsUnique()
                .HasFilter("PromoCode IS NOT NULL");

            builder.Entity<Promotion>()
                .HasOne(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // PromotionProduct
            builder.Entity<PromotionProduct>()
                .HasOne(pp => pp.Promotion)
                .WithMany(p => p.PromotionProducts)
                .HasForeignKey(pp => pp.PromotionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PromotionProduct>()
                .HasOne(pp => pp.Product)
                .WithMany()
                .HasForeignKey(pp => pp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PromotionProduct>()
                .HasOne(pp => pp.ProductVariant)
                .WithMany()
                .HasForeignKey(pp => pp.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            // PromotionUsage
            builder.Entity<PromotionUsage>()
                .HasOne(pu => pu.Promotion)
                .WithMany(p => p.PromotionUsages)
                .HasForeignKey(pu => pu.PromotionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PromotionUsage>()
                .HasOne(pu => pu.Sale)
                .WithMany(s => s.PromotionUsages)
                .HasForeignKey(pu => pu.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PromotionUsage>()
                .HasOne(pu => pu.UsedBy)
                .WithMany()
                .HasForeignKey(pu => pu.UsedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Sale - relation avec Customer
            builder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuration des nouveaux modèles d'achats avancés
            
            // PurchaseOrder
            builder.Entity<PurchaseOrder>()
                .HasIndex(po => po.OrderNumber)
                .IsUnique();

            builder.Entity<PurchaseOrder>()
                .HasOne(po => po.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseOrder>()
                .HasOne(po => po.CreatedBy)
                .WithMany()
                .HasForeignKey(po => po.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseOrder>()
                .HasOne(po => po.ApprovedBy)
                .WithMany()
                .HasForeignKey(po => po.ApprovedById)
                .OnDelete(DeleteBehavior.SetNull);

            // PurchaseOrderItem
            builder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.PurchaseOrder)
                .WithMany(po => po.Items)
                .HasForeignKey(poi => poi.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.Product)
                .WithMany()
                .HasForeignKey(poi => poi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.ProductVariant)
                .WithMany()
                .HasForeignKey(poi => poi.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            // PurchaseReceipt
            builder.Entity<PurchaseReceipt>()
                .HasIndex(pr => pr.ReceiptNumber)
                .IsUnique();

            builder.Entity<PurchaseReceipt>()
                .HasOne(pr => pr.PurchaseOrder)
                .WithMany(po => po.Receipts)
                .HasForeignKey(pr => pr.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseReceipt>()
                .HasOne(pr => pr.CreatedBy)
                .WithMany()
                .HasForeignKey(pr => pr.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseReceipt>()
                .HasOne(pr => pr.VerifiedBy)
                .WithMany()
                .HasForeignKey(pr => pr.VerifiedById)
                .OnDelete(DeleteBehavior.SetNull);

            // PurchaseReceiptItem
            builder.Entity<PurchaseReceiptItem>()
                .HasOne(pri => pri.PurchaseReceipt)
                .WithMany(pr => pr.Items)
                .HasForeignKey(pri => pri.PurchaseReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PurchaseReceiptItem>()
                .HasOne(pri => pri.Product)
                .WithMany()
                .HasForeignKey(pri => pri.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseReceiptItem>()
                .HasOne(pri => pri.ProductVariant)
                .WithMany()
                .HasForeignKey(pri => pri.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

