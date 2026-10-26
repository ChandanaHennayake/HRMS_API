using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.API.Data;
using HRMS.API.Helpers;
using HRMS.API.DTOs.Audit;
using HRMS.API.DTOs.Notification;
using HRMS.API.DTOs.Payroll;
using HRMS.API.DTOs.Salary;
using HRMS.API.Entities;
using HRMS.API.Repository.Payroll;
using HRMS.API.Repository.Salary;
using HRMS.API.Service.Audit;
using HRMS.API.Service.Email;
using HRMS.API.Service.Notification;
using HRMS.API.Service.Payroll;
using HRMS.API.Service.Salary;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace HRMS.Tests
{
    public class IndividualPayrollTests
    {
        private readonly ITestOutputHelper _output;

        public IndividualPayrollTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private class DummyAuditService : IAuditService
        {
            public Task<AuditLogDto> LogAsync(CreateAuditLogDto dto) => Task.FromResult(new AuditLogDto());
            public Task<AuditLogDto?> GetByIdAsync(long id) => Task.FromResult<AuditLogDto?>(null);
            public Task<PagedAuditResultDto> GetPagedAsync(AuditLogQueryDto query) => Task.FromResult(new PagedAuditResultDto());
            public Task<AuditStatsDto> GetStatsAsync() => Task.FromResult(new AuditStatsDto());
            public Task<IEnumerable<string>> GetActionsAsync() => Task.FromResult<IEnumerable<string>>(new List<string>());
            public Task<IEnumerable<string>> GetEntitiesAsync() => Task.FromResult<IEnumerable<string>>(new List<string>());
            public Task LogSecurityEventAsync(int? userId, string? userName, string action, string description, string? ipAddress, bool isSuccess = true) => Task.CompletedTask;
            public Task LogEntityActionAsync(int? userId, string? userName, string action, string entityName, string entityId, string description, object? oldValues = null, object? newValues = null, string severity = "Info") => Task.CompletedTask;
        }

        private class DummyNotificationService : INotificationService
        {
            public Task<NotificationDto> SendNotificationAsync(CreateNotificationDto dto) => Task.FromResult(new NotificationDto());
            public Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, int limit = 50) => Task.FromResult<IEnumerable<NotificationDto>>(new List<NotificationDto>());
            public Task<NotificationSummaryDto> GetSummaryAsync(int userId) => Task.FromResult(new NotificationSummaryDto());
            public Task MarkAsReadAsync(long notificationId) => Task.CompletedTask;
            public Task MarkAllAsReadAsync(int userId) => Task.CompletedTask;
            public Task NotifyLeaveDecisionAsync(long employeeId, string leaveTypeName, bool isApproved, string? comment) => Task.CompletedTask;
            public Task NotifyAttendanceCorrectionDecisionAsync(long employeeId, DateOnly attendanceDate, bool isApproved, string? comment) => Task.CompletedTask;
            public Task NotifyPayrollPublishedAsync(long employeeId, string periodName, decimal netSalary, long payslipId) => Task.CompletedTask;
        }

        private class DummyEmailService : IEmailService
        {
            public Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string htmlBody, string? plainTextBody = null) => Task.FromResult(true);
            public Task<bool> SendLeaveDecisionEmailAsync(string recipientEmail, string employeeName, string leaveType, DateOnly fromDate, DateOnly toDate, decimal totalDays, bool isApproved, string? reviewerComments) => Task.FromResult(true);
            public Task<bool> SendAttendanceCorrectionDecisionEmailAsync(string recipientEmail, string employeeName, DateOnly attendanceDate, bool isApproved, string? reviewerComments) => Task.FromResult(true);
            public Task<bool> SendPayslipPublishedEmailAsync(string recipientEmail, string employeeName, string periodName, decimal netSalary) => Task.FromResult(true);
        }

        private class DummySalaryService : ISalaryService
        {
            public SalaryCalculationResultDto CalculateSalary(SalaryCalculationRequestDto request) => new();
            public Task<EmployeeSalaryDto?> GetEmployeeSalaryAsync(long employeeId) => Task.FromResult<EmployeeSalaryDto?>(null);
            public Task<EmployeeSalaryDto> SaveEmployeeSalaryAsync(SaveEmployeeSalaryDto request) => Task.FromResult(new EmployeeSalaryDto());
            public Task<IEnumerable<SalaryComponentDto>> GetComponentsAsync(long companyId) => Task.FromResult<IEnumerable<SalaryComponentDto>>(new List<SalaryComponentDto>());
            public Task<SalaryComponentDto> CreateComponentAsync(SalaryComponentDto request) => Task.FromResult(new SalaryComponentDto());
            public Task<IEnumerable<SalaryStructure>> GetStructuresAsync(long companyId) => Task.FromResult<IEnumerable<SalaryStructure>>(new List<SalaryStructure>());
        }

        [Fact]
        public async Task Test_SaveIndividualPayroll_Repro()
        {
            using var context = new DefaultContext();
            var canConnect = await context.Database.CanConnectAsync();
            _output.WriteLine($"Database CanConnect: {canConnect}");
            if (!canConnect) return;

            await DatabaseInitializer.EnsureSchemaUpToDateAsync(context, Microsoft.Extensions.Logging.Abstractions.NullLogger<IndividualPayrollTests>.Instance);

            var emp = await context.Employees.FirstOrDefaultAsync();
            if (emp == null)
            {
                _output.WriteLine("No employees found in DB.");
                return;
            }
            _output.WriteLine($"Testing with Employee Id: {emp.Id}, Name: {emp.FirstName} {emp.LastName}");

            var payrollRepo = new PayrollRepository(context);
            var salaryRepo = new SalaryRepository(context);
            var salaryService = new DummySalaryService();
            var auditService = new DummyAuditService();
            var notifService = new DummyNotificationService();
            var emailService = new DummyEmailService();

            var payrollService = new PayrollService(payrollRepo, salaryService, salaryRepo, context, notifService, auditService, emailService);

            var req = new IndividualPayrollRequestDto
            {
                EmployeeId = emp.Id,
                Year = 2026,
                Month = 10,
                StartDate = new DateOnly(2026, 10, 1),
                EndDate = new DateOnly(2026, 10, 31),
                BaseSalary = 50000m,
                WorkingDaysDivisor = 26,
                PerDayRate = 1923.08m,
                NoPayDays = 0,
                NoPayDeduction = 0,
                OvertimeHours = 0,
                OvertimePay = 0,
                PaymentStatus = "Unpaid",
                Allowances = new List<PayrollManualItemDto>
                {
                    new() { Name = "Transport Allowance", Amount = 5000m, IsEpfApplicable = false }
                },
                Deductions = new List<PayrollManualItemDto>()
            };

            try
            {
                var result = await payrollService.SaveIndividualPayrollAsync(req, 1);
                _output.WriteLine($"Successfully saved payslip ID: {result.Id}");

                // Now test UPDATE path (calling save again with modified allowances/deductions)
                req.BaseSalary = 55000m;
                req.OvertimeHours = 5;
                req.OvertimeRate = 350;
                req.OvertimePay = 1750;
                req.Allowances.Add(new PayrollManualItemDto { Name = "Special Project Performance Allowance", Amount = 10000m });
                req.Deductions.Add(new PayrollManualItemDto { Name = "Advance Loan Deductions", Amount = 2000m });

                var updatedResult = await payrollService.SaveIndividualPayrollAsync(req, 1);
                _output.WriteLine($"Successfully updated payslip ID: {updatedResult.Id}, Gross: {updatedResult.GrossEarnings}, Net: {updatedResult.NetSalary}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"EXCEPTION: {ex.GetType().FullName}: {ex.Message}");
                var inner = ex.InnerException;
                while (inner != null)
                {
                    _output.WriteLine($"INNER EXCEPTION: {inner.GetType().FullName}: {inner.Message}");
                    inner = inner.InnerException;
                }
                throw;
            }
        }
    }
}
