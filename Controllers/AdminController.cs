using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;
using SwiftStock.Services;

namespace SwiftStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<User> userManager, 
            ApplicationDbContext context, 
            IAuditService auditService,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _context = context;
            _auditService = auditService;
            _logger = logger;
        }

        [HttpPost("users/{userId}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(string userId, [FromQuery] bool isActive)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Utilisateur non trouvé");
                }

                var oldStatus = user.IsActive;
                user.IsActive = isActive;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Enregistrer l'audit
                    await _auditService.LogAsync(
                        AuditAction.Update,
                        "User",
                        user.Id,
                        $"{user.FirstName} {user.LastName}",
                        $"Statut changé de {(oldStatus ? "Actif" : "Inactif")} à {(isActive ? "Actif" : "Inactif")}",
                        oldStatus.ToString(),
                        isActive.ToString(),
                        User.Identity?.Name,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );

                    _logger.LogInformation("Statut de l'utilisateur {UserId} changé à {Status}", userId, isActive);
                    return Ok(new { success = true, message = "Statut mis à jour avec succès" });
                }

                return BadRequest(new { success = false, message = "Erreur lors de la mise à jour" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de statut de l'utilisateur {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpPost("users/{userId}/reset-password")]
        public async Task<IActionResult> ResetUserPassword(string userId, [FromBody] ResetPasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Utilisateur non trouvé");
                }

                // Supprimer l'ancien mot de passe
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                {
                    return BadRequest(new { success = false, message = "Erreur lors de la suppression de l'ancien mot de passe" });
                }

                // Ajouter le nouveau mot de passe
                var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
                if (addResult.Succeeded)
                {
                    user.MustChangePassword = true;
                    user.LastPasswordChange = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    // Enregistrer l'audit
                    await _auditService.LogAsync(
                        AuditAction.Update,
                        "User",
                        user.Id,
                        $"{user.FirstName} {user.LastName}",
                        "Mot de passe réinitialisé",
                        null,
                        "Nouveau mot de passe défini",
                        User.Identity?.Name,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );

                    _logger.LogInformation("Mot de passe de l'utilisateur {UserId} réinitialisé", userId);
                    return Ok(new { success = true, message = "Mot de passe réinitialisé avec succès" });
                }

                return BadRequest(new { success = false, message = "Erreur lors de la définition du nouveau mot de passe" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la réinitialisation du mot de passe de l'utilisateur {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpGet("users/{userId}/audit-logs")]
        public async Task<IActionResult> GetUserAuditLogs(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var logs = await _auditService.GetAuditLogsAsync(userId, null, null, null, page, pageSize);
                var totalCount = await _auditService.GetAuditLogsCountAsync(userId, null, null, null);

                return Ok(new
                {
                    logs = logs,
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des logs d'audit pour l'utilisateur {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }

        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = new
                {
                    totalUsers = await _userManager.Users.CountAsync(),
                    activeUsers = await _userManager.Users.CountAsync(u => u.IsActive),
                    totalProducts = await _context.Products.CountAsync(p => p.IsActive),
                    lowStockProducts = await _context.Products.CountAsync(p => p.IsActive && p.CurrentStock <= p.MinStockLevel),
                    todaySales = await _context.Sales
                        .Where(s => s.SaleDate.Date == DateTime.Today && s.Status == SaleStatus.Completed)
                        .CountAsync(),
                    todayRevenue = await _context.Sales
                        .Where(s => s.SaleDate.Date == DateTime.Today && s.Status == SaleStatus.Completed)
                        .SumAsync(s => s.TotalAmount)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques du tableau de bord");
                return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
            }
        }
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}

