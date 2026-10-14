using HRMS.API.DTOs.Dashboard;

namespace HRMS.API.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<AdminSummaryDto> GetAdminSummaryAsync(long? companyId = null);
        Task<ManagerSummaryDto> GetManagerSummaryAsync(long branchId);
    }
}
