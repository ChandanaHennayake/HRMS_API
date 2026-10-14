using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.API.DTOs.Audit;

namespace HRMS.API.Service.Audit
{
    public interface IAuditService
    {
        Task<AuditLogDto> LogAsync(CreateAuditLogDto dto);
        Task<AuditLogDto?> GetByIdAsync(long id);
        Task<PagedAuditResultDto> GetPagedAsync(AuditLogQueryDto query);
        Task<AuditStatsDto> GetStatsAsync();
        Task<IEnumerable<string>> GetActionsAsync();
        Task<IEnumerable<string>> GetEntitiesAsync();
        Task LogSecurityEventAsync(int? userId, string? userName, string action, string description, string? ipAddress, bool isSuccess = true);
        Task LogEntityActionAsync(int? userId, string? userName, string action, string entityName, string entityId, string description, object? oldValues = null, object? newValues = null, string severity = "Info");
    }
}
