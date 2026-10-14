using System;
using System.Text;
using System.Threading.Tasks;
using HRMS.API.DTOs.Report;
using HRMS.API.Repository.Report;
using Microsoft.Extensions.Logging;

namespace HRMS.API.Service.Report
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repository;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IReportRepository repository, ILogger<ReportService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<AttendanceReportResponseDto> GetAttendanceReportAsync(AttendanceReportFilterDto filter)
        {
            return await _repository.GetAttendanceReportAsync(filter);
        }

        public async Task<byte[]> GenerateAttendanceCsvAsync(AttendanceReportFilterDto filter)
        {
            var data = await _repository.GetAttendanceReportAsync(filter);
            var sb = new StringBuilder();

            // CSV Header
            sb.AppendLine("Date,Employee Code,Employee Name,Branch,Department,Check-In,Check-Out,Worked Hours,Is Late,Late Minutes,OT Minutes,Status");

            foreach (var item in data.Items)
            {
                var checkIn = item.CheckInTime?.ToString("HH:mm:ss") ?? "--:--";
                var checkOut = item.CheckOutTime?.ToString("HH:mm:ss") ?? "--:--";

                sb.AppendLine($"\"{item.Date:yyyy-MM-dd}\",\"{EscapeCsv(item.EmployeeCode)}\",\"{EscapeCsv(item.EmployeeName)}\",\"{EscapeCsv(item.BranchName)}\",\"{EscapeCsv(item.DepartmentName)}\",\"{checkIn}\",\"{checkOut}\",{item.WorkedHours},{(item.IsLate ? "Yes" : "No")},{item.LateMinutes},{item.OTMinutes},\"{item.Status}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<LeaveReportResponseDto> GetLeaveReportAsync(LeaveReportFilterDto filter)
        {
            return await _repository.GetLeaveReportAsync(filter);
        }

        public async Task<byte[]> GenerateLeaveCsvAsync(LeaveReportFilterDto filter)
        {
            var data = await _repository.GetLeaveReportAsync(filter);
            var sb = new StringBuilder();

            // CSV Header
            sb.AppendLine("Request ID,Employee Code,Employee Name,Branch,Leave Type,From Date,To Date,Total Days,Is Paid,Status,Approved At");

            foreach (var item in data.Items)
            {
                var approvedAt = item.ApprovedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";

                sb.AppendLine($"{item.Id},\"{EscapeCsv(item.EmployeeCode)}\",\"{EscapeCsv(item.EmployeeName)}\",\"{EscapeCsv(item.BranchName)}\",\"{EscapeCsv(item.LeaveTypeName)}\",\"{item.FromDate:yyyy-MM-dd}\",\"{item.ToDate:yyyy-MM-dd}\",{item.TotalDays},{(item.IsPaid ? "Yes" : "No")},\"{item.Status}\",\"{approvedAt}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<PayrollReportResponseDto> GetPayrollReportAsync(PayrollReportFilterDto filter)
        {
            return await _repository.GetPayrollReportAsync(filter);
        }

        public async Task<byte[]> GeneratePayrollCsvAsync(PayrollReportFilterDto filter)
        {
            var data = await _repository.GetPayrollReportAsync(filter);
            var sb = new StringBuilder();

            // CSV Header
            sb.AppendLine("Payslip ID,Employee Code,Employee Name,Branch,Department,Base Salary (LKR),Gross Earnings (LKR),Total Deductions (LKR),Net Salary (LKR),Status");

            foreach (var item in data.Items)
            {
                sb.AppendLine($"{item.PayslipId},\"{EscapeCsv(item.EmployeeCode)}\",\"{EscapeCsv(item.EmployeeName)}\",\"{EscapeCsv(item.BranchName)}\",\"{EscapeCsv(item.DepartmentName)}\",{item.BaseSalary:F2},{item.GrossEarnings:F2},{item.TotalDeductions:F2},{item.NetSalary:F2},\"{item.PaymentStatus}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<HeadcountReportDto> GetHeadcountReportAsync(int? branchId = null)
        {
            return await _repository.GetHeadcountReportAsync(branchId);
        }

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("\"", "\"\"");
        }
    }
}
