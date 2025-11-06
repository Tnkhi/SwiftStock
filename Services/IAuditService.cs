using SwiftStock.Models;

namespace SwiftStock.Services
{
    public interface IAuditService
    {
        Task LogAsync(AuditAction action, string entityType, string? entityId, string? entityName, 
                     string? description = null, string? oldValues = null, string? newValues = null, 
                     string? userId = null, string? ipAddress = null, string? userAgent = null);
        
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? userId = null, string? entityType = null, 
                                                     DateTime? fromDate = null, DateTime? toDate = null, 
                                                     int page = 1, int pageSize = 50);
        
        Task<int> GetAuditLogsCountAsync(string? userId = null, string? entityType = null, 
                                        DateTime? fromDate = null, DateTime? toDate = null);
    }
}

