using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.API.Data;
using HRMS.API.DTOs.Audit;
using HRMS.API.DTOs.Employee;
using HRMS.API.DTOs.Notification;
using HRMS.API.DTOs.ProfileChange;
using HRMS.API.DTOs.User;
using HRMS.API.Entities;
using HRMS.API.Interfaces.Repositories;
using HRMS.API.Repositories;
using HRMS.API.Service.Audit;
using HRMS.API.Service.Notification;
using HRMS.API.Service.ProfileChange;
using HRMS.API.Service.User;
using HRMS.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HRMS.Tests
{
    public class ProfileChangeTests
    {
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

        private class DummyUserService : IUserService
        {
            public Task<int> CreateAsync(CreateUserRequest request) => Task.FromResult(1);
            public Task<UserResponse?> GetByIdAsync(int id) => Task.FromResult<UserResponse?>(null);
            public Task<UserProfileResponse?> GetProfileAsync(int userId) => Task.FromResult<UserProfileResponse?>(null);
            public Task<List<UserResponse>> GetAllAsync() => Task.FromResult(new List<UserResponse>());
            public Task ChangePasswordAsync(ChangePasswordRequest request) => Task.CompletedTask;
            public Task AdminResetPasswordAsync(AdminResetPasswordRequest request) => Task.CompletedTask;
            public Task UpdateAsync(int id, UpdateUserRequest request) => Task.CompletedTask;
            public Task DeleteAsync(int id) => Task.CompletedTask;
        }

        private DefaultContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<DefaultContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new DefaultContext(options);
        }

        [Fact]
        public async Task AdminEmployeeUpdate_ShouldSynchronizeNamesAndDetailsToAppUserTable()
        {
            // Arrange
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            var empRepo = new EmployeeRepository(db);
            var empService = new EmployeeService(empRepo, new DummyUserService(), db);

            var employee = new Employee
            {
                Id = 10,
                CompanyId = 1,
                BranchId = 1,
                EmployeeCode = "EMP010",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                Phone = "0112233445",
                JoinedDate = new DateOnly(2025, 1, 1),
                IsActive = true
            };
            db.Employees.Add(employee);

            var user = new AppUser
            {
                UserId = 20,
                CompanyId = 1,
                BranchId = 1,
                RoleId = 4,
                EmployeeId = 10,
                Username = "johndoe",
                Email = "john.doe@test.com",
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = "hash",
                IsActive = true
            };
            db.AppUsers.Add(user);
            await db.SaveChangesAsync();

            // Act - Admin edits employee via UpdateAsync
            var updateDto = new UpdateEmployeeDto
            {
                CompanyId = 1,
                BranchId = 2,
                EmployeeCode = "EMP010",
                FirstName = "Jonathan",
                LastName = "Doeman",
                Email = "jonathan.doeman@test.com",
                Phone = "0771234567",
                JoinedDate = new DateOnly(2025, 1, 1),
                IsActive = true,
                BankName = "Bank of Ceylon",
                BankAccountNo = "1234567890"
            };

            var updatedEmployee = await empService.UpdateAsync(10, updateDto);

            // Assert
            Assert.NotNull(updatedEmployee);
            Assert.Equal("Jonathan", updatedEmployee.FirstName);
            Assert.Equal("Doeman", updatedEmployee.LastName);

            var syncedUser = await db.AppUsers.FirstOrDefaultAsync(u => u.UserId == 20);
            Assert.NotNull(syncedUser);
            Assert.Equal("Jonathan", syncedUser.FirstName);
            Assert.Equal("Doeman", syncedUser.LastName);
            Assert.Equal("jonathan.doeman@test.com", syncedUser.Email);
            Assert.Equal("0771234567", syncedUser.Phone);
            Assert.Equal(2, syncedUser.BranchId);
        }

        [Fact]
        public async Task EmployeeProfileChange_SubmitAndApprove_ShouldUpdateEmployeeUserAndSalary()
        {
            // Arrange
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            var profileService = new ProfileChangeService(db, new DummyNotificationService(), new DummyAuditService(), NullLogger<ProfileChangeService>.Instance);

            var employee = new Employee
            {
                Id = 15,
                CompanyId = 1,
                BranchId = 1,
                EmployeeCode = "EMP015",
                FirstName = "Alice",
                LastName = "Brown",
                Email = "alice.brown@test.com",
                Phone = "0711111111",
                BankName = "Old Bank",
                BankAccountNo = "1111111111",
                JoinedDate = new DateOnly(2024, 1, 1),
                IsActive = true
            };
            db.Employees.Add(employee);

            var user = new AppUser
            {
                UserId = 35,
                CompanyId = 1,
                RoleId = 4,
                EmployeeId = 15,
                Username = "aliceb",
                Email = "alice.brown@test.com",
                FirstName = "Alice",
                LastName = "Brown",
                PasswordHash = "hash",
                IsActive = true
            };
            db.AppUsers.Add(user);

            var salary = new EmployeeSalary
            {
                Id = 1,
                EmployeeId = 15,
                BaseSalary = 50000,
                BankName = "Old Bank",
                BankAccountNo = "1111111111",
                EffectiveFrom = new DateOnly(2024, 1, 1),
                IsActive = true
            };
            db.EmployeeSalaries.Add(salary);

            var adminUser = new AppUser
            {
                UserId = 1,
                CompanyId = 1,
                RoleId = 1,
                Username = "admin",
                Email = "admin@test.com",
                FirstName = "Super",
                LastName = "Admin",
                PasswordHash = "hash",
                IsActive = true
            };
            db.AppUsers.Add(adminUser);
            await db.SaveChangesAsync();

            // Act 1: Employee submits profile change request
            var submitDto = new SubmitProfileChangeRequestDto
            {
                EmployeeId = 15,
                FirstName = "Alicia",
                LastName = "Smith",
                Phone = "0779998877",
                BankName = "Commercial Bank",
                BankAccountNo = "8888888888",
                BankBranch = "Colombo 03",
                Reason = "Updated marital name and salary account"
            };

            var submittedRequest = await profileService.SubmitRequestAsync(submitDto, currentUserId: 35);

            // Assert Submission
            Assert.NotNull(submittedRequest);
            Assert.Equal((short)0, submittedRequest.Status);
            Assert.Equal("Pending", submittedRequest.StatusText);
            Assert.Equal("Alicia", submittedRequest.FirstName);
            Assert.Equal("Alice", submittedRequest.CurrentFirstName);
            Assert.Equal("Commercial Bank", submittedRequest.BankName);
            Assert.Equal("Old Bank", submittedRequest.CurrentBankName);

            // Act 2: Admin approves request
            var reviewDto = new ReviewProfileChangeDto
            {
                ReviewerUserId = 1,
                Comment = "Verified with bank passbook and ID proof"
            };

            var approvedRequest = await profileService.ApproveRequestAsync(submittedRequest.Id, reviewDto, reviewerUserId: 1);

            // Assert Approval
            Assert.Equal((short)1, approvedRequest.Status);
            Assert.Equal("Approved", approvedRequest.StatusText);
            Assert.Equal("Super Admin", approvedRequest.ReviewerName);

            // Verify Employee Table Updated
            var updatedEmp = await db.Employees.FindAsync((long)15);
            Assert.NotNull(updatedEmp);
            Assert.Equal("Alicia", updatedEmp.FirstName);
            Assert.Equal("Smith", updatedEmp.LastName);
            Assert.Equal("0779998877", updatedEmp.Phone);
            Assert.Equal("Commercial Bank", updatedEmp.BankName);
            Assert.Equal("8888888888", updatedEmp.BankAccountNo);
            Assert.Equal("Colombo 03", updatedEmp.BankBranch);

            // Verify AppUser (User Table) Updated
            var updatedUser = await db.AppUsers.FindAsync(35);
            Assert.NotNull(updatedUser);
            Assert.Equal("Alicia", updatedUser.FirstName);
            Assert.Equal("Smith", updatedUser.LastName);
            Assert.Equal("0779998877", updatedUser.Phone);

            // Verify EmployeeSalary Table Updated
            var updatedSalary = await db.EmployeeSalaries.FindAsync((long)1);
            Assert.NotNull(updatedSalary);
            Assert.Equal("Commercial Bank", updatedSalary.BankName);
            Assert.Equal("8888888888", updatedSalary.BankAccountNo);
        }

        [Fact]
        public async Task EmployeeProfileChange_Reject_ShouldNotAlterRecords()
        {
            // Arrange
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            var profileService = new ProfileChangeService(db, new DummyNotificationService(), new DummyAuditService(), NullLogger<ProfileChangeService>.Instance);

            var employee = new Employee
            {
                Id = 22,
                CompanyId = 1,
                EmployeeCode = "EMP022",
                FirstName = "Bob",
                LastName = "Miller",
                Email = "bob@test.com",
                BankName = "Original Bank",
                BankAccountNo = "0000000000",
                JoinedDate = new DateOnly(2024, 1, 1),
                IsActive = true
            };
            db.Employees.Add(employee);

            var user = new AppUser
            {
                UserId = 44,
                CompanyId = 1,
                EmployeeId = 22,
                Username = "bobm",
                Email = "bob@test.com",
                FirstName = "Bob",
                LastName = "Miller",
                PasswordHash = "hash",
                IsActive = true
            };
            db.AppUsers.Add(user);
            await db.SaveChangesAsync();

            var submitDto = new SubmitProfileChangeRequestDto
            {
                EmployeeId = 22,
                FirstName = "Robert",
                BankName = "Fraudulent Bank",
                BankAccountNo = "9999999999",
                Reason = "Change name"
            };
            var request = await profileService.SubmitRequestAsync(submitDto, 44);

            // Act - Admin rejects
            var reviewDto = new ReviewProfileChangeDto
            {
                ReviewerUserId = 1,
                Comment = "Documents missing"
            };
            var rejected = await profileService.RejectRequestAsync(request.Id, reviewDto, 1);

            // Assert
            Assert.Equal((short)2, rejected.Status);
            Assert.Equal("Rejected", rejected.StatusText);
            Assert.Equal("Documents missing", rejected.ReviewerComment);

            var checkEmp = await db.Employees.FindAsync((long)22);
            Assert.NotNull(checkEmp);
            Assert.Equal("Bob", checkEmp.FirstName);
            Assert.Equal("Original Bank", checkEmp.BankName);

            var checkUser = await db.AppUsers.FindAsync(44);
            Assert.NotNull(checkUser);
            Assert.Equal("Bob", checkUser.FirstName);
        }
    }
}
