using System.Threading.Tasks;
using HRMS.API.DTOs.Report;

namespace HRMS.API.Service.Report
{
    public interface IReportService
    {
        Task<AttendanceReportResponseDto> GetAttendanceReportAsync(AttendanceReportFilterDto filter);
        Task<byte[]> GenerateAttendanceCsvAsync(AttendanceReportFilterDto filter);

        Task<LeaveReportResponseDto> GetLeaveReportAsync(LeaveReportFilterDto filter);
        Task<byte[]> GenerateLeaveCsvAsync(LeaveReportFilterDto filter);

        Task<PayrollReportResponseDto> GetPayrollReportAsync(PayrollReportFilterDto filter);
        Task<byte[]> GeneratePayrollCsvAsync(PayrollReportFilterDto filter);

        Task<HeadcountReportDto> GetHeadcountReportAsync(int? branchId = null);
    }
}
