using System;
using System.Threading.Tasks;
using HRMS.API.DTOs.Employee;
using HRMS.API.DTOs.User;
using HRMS.API.Entities;
using Xunit;

namespace HRMS.Tests
{
    public class UserSecurityAndAdminPasswordTests
    {
        [Fact]
        public void AppUser_Supports_ClearPassword_And_IsDeleted()
        {
            var user = new AppUser
            {
                UserId = 42,
                Username = "sarah.connor",
                Email = "sarah@alienhr.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Secret@123"),
                ClearPassword = "Secret@123",
                PasswordSalt = "Secret@123",
                IsActive = true,
                IsDeleted = false
            };

            Assert.Equal("Secret@123", user.ClearPassword);
            Assert.Equal("Secret@123", user.PasswordSalt);
            Assert.False(user.IsDeleted);
            Assert.True(BCrypt.Net.BCrypt.Verify("Secret@123", user.PasswordHash));
        }

        [Fact]
        public void UserResponse_Exposes_ClearPassword_And_DeletedStatus_To_Admin()
        {
            var response = new UserResponse
            {
                UserId = 5,
                EmployeeId = 12,
                Username = "john.doe",
                Email = "john@alienhr.com",
                FullName = "John Doe",
                Role = "Employee",
                RoleId = 4,
                IsActive = true,
                IsDeleted = false,
                ClearPassword = "MySecurePassword123"
            };

            Assert.Equal(5, response.UserId);
            Assert.Equal(12, response.EmployeeId);
            Assert.Equal("MySecurePassword123", response.ClearPassword);
            Assert.False(response.IsDeleted);
        }

        [Fact]
        public void EmployeeDto_Includes_Credentials_For_Admin_Management()
        {
            var empDto = new EmployeeDto
            {
                Id = 101,
                EmployeeCode = "EMP101",
                FirstName = "Kamal",
                LastName = "Silva",
                Email = "kamal@alienhr.com",
                UserId = 7,
                Username = "kamal.silva",
                Password = "KamalPassword@456"
            };

            Assert.Equal(7, empDto.UserId);
            Assert.Equal("kamal.silva", empDto.Username);
            Assert.Equal("KamalPassword@456", empDto.Password);
        }

        [Fact]
        public void UpdateEmployeeDto_Supports_Password_And_Username_Updates()
        {
            var updateDto = new UpdateEmployeeDto
            {
                CompanyId = 1,
                BranchId = 1,
                EmployeeCode = "EMP101",
                FirstName = "Kamal",
                LastName = "Silva",
                JoinedDate = new DateOnly(2025, 1, 1),
                IsActive = true,
                Username = "kamal.updated",
                Password = "NewPassword@789",
                RoleId = 2
            };

            Assert.Equal("kamal.updated", updateDto.Username);
            Assert.Equal("NewPassword@789", updateDto.Password);
            Assert.Equal(2, updateDto.RoleId);
        }

        [Fact]
        public void AdminResetPasswordRequest_Encapsulates_UserId_And_NewPassword()
        {
            var req = new AdminResetPasswordRequest
            {
                UserId = 15,
                NewPassword = "AdminChangedSecret123"
            };

            Assert.Equal(15, req.UserId);
            Assert.Equal("AdminChangedSecret123", req.NewPassword);
            Assert.True(req.NewPassword.Length >= 6);
        }
    }
}
