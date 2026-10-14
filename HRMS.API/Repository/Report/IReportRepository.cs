using System.Threading.Tasks;
using HRMS.API.DTOs.Report;

namespace HRMS.API.Repository.Report
{
    public interface IReportRepository
    {
        Task<AttendanceReportResponseDto> GetAttendanceReportAsync(AttendanceReportFilterDto filter);
        Task<LeaveReportResponseDto> GetLeaveReportAsync(LeaveReportFilterDto filter);
        Task<PayrollReportResponseDto> GetPayrollReportAsync(PayrollReportFilterDto filter);
        Task<HeadcountReportDto> GetHeadcountReportAsync(int? branchId = null);
    }
}
