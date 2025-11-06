using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPurchases { get; set; } = 0;

        public int TotalOrders { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal CreditLimit { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal OutstandingBalance { get; set; } = 0;

        public DateTime? LastPurchaseDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(450)]
        public string? CreatedById { get; set; }

        // Navigation properties
        public virtual User? CreatedBy { get; set; }
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}

