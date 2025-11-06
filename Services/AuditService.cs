using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(AuditAction action, string entityType, string? entityId, string? entityName, 
                                 string? description = null, string? oldValues = null, string? newValues = null, 
                                 string? userId = null, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityName = entityName,
                    Description = description,
                    OldValues = oldValues,
                    NewValues = newValues,
                    UserId = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement de l'audit log pour {EntityType} {EntityId}", 
                    entityType, entityId);
            }
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? userId = null, string? entityType = null, 
                                                                 DateTime? fromDate = null, DateTime? toDate = null, 
                                                                 int page = 1, int pageSize = 50)
        {
            var query = _context.AuditLogs
                .Include(al => al.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(al => al.UserId == userId);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(al => al.EntityType == entityType);

            if (fromDate.HasValue)
                query = query.Where(al => al.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(al => al.Timestamp <= toDate.Value);

            return await query
                .OrderByDescending(al => al.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetAuditLogsCountAsync(string? userId = null, string? entityType = null, 
                                                     DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(al => al.UserId == userId);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(al => al.EntityType == entityType);

            if (fromDate.HasValue)
                query = query.Where(al => al.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(al => al.Timestamp <= toDate.Value);

            return await query.CountAsync();
        }
    }
}

