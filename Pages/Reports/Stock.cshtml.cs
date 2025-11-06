using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SwiftStock.Pages.Reports
{
    public class StockModel : PageModel
    {
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Today;

        public void OnGet()
        {
            // Logique d'initialisation si nécessaire
        }
    }
}

