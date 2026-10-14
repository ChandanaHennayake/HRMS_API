using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.API.DTOs.Audit;
using HRMS.API.Entities;

namespace HRMS.API.Repository.Audit
{
    public interface IAuditRepository
    {
        Task<AuditLog> CreateAsync(AuditLog log);
        Task<AuditLog?> GetByIdAsync(long id);
        Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(AuditLogQueryDto query);
        Task<AuditStatsDto> GetStatsAsync();
        Task<IEnumerable<string>> GetDistinctActionsAsync();
        Task<IEnumerable<string>> GetDistinctEntitiesAsync();
    }
}
