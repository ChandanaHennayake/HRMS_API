using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HRMS.API.DTOs.Report;
using HRMS.API.Repository.Report;
using HRMS.API.Service.Report;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HRMS.Tests
{
    public class ReportServiceTests
    {
        private class FakeReportRepository : IReportRepository
        {
            public Task<AttendanceReportResponseDto> GetAttendanceReportAsync(AttendanceReportFilterDto filter)
            {
                var response = new AttendanceReportResponseDto
                {
                    Summary = new AttendanceReportSummaryDto
                    {
                        TotalRecords = 1,
                        PresentCount = 1,
                        PunctualityRate = 100.0,
                        TotalWorkedHours = 8.5
                    },
                    Items = new List<AttendanceReportItemDto>
                    {
                        new()
                        {
                            Date = new DateOnly(2026, 10, 14),
                            EmployeeCode = "EMP001",
                            EmployeeName = "Jane Doe",
                            BranchName = "Colombo HQ",
                            DepartmentName = "Engineering",
                            WorkedHours = 8.5,
                            IsLate = false,
                            LateMinutes = 0,
                            OTMinutes = 30,
                            Status = "Present"
                        }
                    }
                };
                return Task.FromResult(response);
            }

            public Task<LeaveReportResponseDto> GetLeaveReportAsync(LeaveReportFilterDto filter)
            {
                var response = new LeaveReportResponseDto
                {
                    Summary = new LeaveReportSummaryDto
                    {
                        TotalRequests = 1,
                        ApprovedDays = 2.0m,
                        PaidDays = 2.0m
                    },
                    Items = new List<LeaveReportItemDto>
                    {
                        new()
                        {
                            Id = 1,
                            EmployeeCode = "EMP002",
                            EmployeeName = "John Smith",
                            BranchName = "Kandy Branch",
                            LeaveTypeName = "Annual Leave",
                            FromDate = new DateOnly(2026, 10, 10),
                            ToDate = new DateOnly(2026, 10, 11),
                            TotalDays = 2.0m,
                            IsPaid = true,
                            Status = "Approved"
                        }
                    }
                };
                return Task.FromResult(response);
            }

            public Task<PayrollReportResponseDto> GetPayrollReportAsync(PayrollReportFilterDto filter)
            {
                var response = new PayrollReportResponseDto
                {
                    Summary = new PayrollReportSummaryDto
                    {
                        PeriodName = "October 2026",
                        EmployeeCount = 1,
                        TotalGrossSalary = 100000m,
                        TotalNetSalary = 92000m,
                        TotalEPFEmployer = 12000m,
                        TotalETFEmployer = 3000m,
                        TotalCompanyCost = 115000m
                    },
                    Items = new List<PayrollReportItemDto>
                    {
                        new()
                        {
                            PayslipId = 10,
                            EmployeeCode = "EMP001",
                            EmployeeName = "Jane Doe",
                            BranchName = "Colombo HQ",
                            DepartmentName = "Engineering",
                            BaseSalary = 80000m,
                            GrossEarnings = 100000m,
                            TotalDeductions = 8000m,
                            NetSalary = 92000m,
                            PaymentStatus = "Paid"
                        }
                    }
                };
                return Task.FromResult(response);
            }

            public Task<HeadcountReportDto> GetHeadcountReportAsync(int? branchId = null)
            {
                return Task.FromResult(new HeadcountReportDto
                {
                    TotalEmployees = 10,
                    ActiveEmployees = 9,
                    InactiveEmployees = 1
                });
            }
        }

        [Fact]
        public async Task GenerateAttendanceCsv_Produces_Valid_Csv_Content()
        {
            var fakeRepo = new FakeReportRepository();
            var service = new ReportService(fakeRepo, NullLogger<ReportService>.Instance);

            var bytes = await service.GenerateAttendanceCsvAsync(new AttendanceReportFilterDto());
            var csv = Encoding.UTF8.GetString(bytes);

            Assert.Contains("Date,Employee Code,Employee Name,Branch,Department", csv);
            Assert.Contains("Jane Doe", csv);
            Assert.Contains("Colombo HQ", csv);
            Assert.Contains("8.5", csv);
        }

        [Fact]
        public async Task GeneratePayrollCsv_Formats_Currency_Columns()
        {
            var fakeRepo = new FakeReportRepository();
            var service = new ReportService(fakeRepo, NullLogger<ReportService>.Instance);

            var bytes = await service.GeneratePayrollCsvAsync(new PayrollReportFilterDto());
            var csv = Encoding.UTF8.GetString(bytes);

            Assert.Contains("Payslip ID,Employee Code,Employee Name,Branch", csv);
            Assert.Contains("100000.00", csv);
            Assert.Contains("92000.00", csv);
            Assert.Contains("Jane Doe", csv);
        }

        [Fact]
        public async Task GenerateLeaveCsv_Produces_Valid_Leave_Rows()
        {
            var fakeRepo = new FakeReportRepository();
            var service = new ReportService(fakeRepo, NullLogger<ReportService>.Instance);

            var bytes = await service.GenerateLeaveCsvAsync(new LeaveReportFilterDto());
            var csv = Encoding.UTF8.GetString(bytes);

            Assert.Contains("Request ID,Employee Code,Employee Name", csv);
            Assert.Contains("John Smith", csv);
            Assert.Contains("Annual Leave", csv);
            Assert.Contains("Approved", csv);
        }
    }
}
