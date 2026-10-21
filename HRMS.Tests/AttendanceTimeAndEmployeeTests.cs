using System;
using ABANS_BAN.DTOs.Attendance;
using HRMS.API.DTOs.Authentication;
using HRMS.API.DTOs.Employee;
using HRMS.API.Entities;
using HRMS.API.Helpers;
using Xunit;

namespace HRMS.Tests
{
    public class AttendanceTimeAndEmployeeTests
    {
        [Fact]
        public void DateTimeHelper_Resolves_Sri_Lanka_Standard_Time()
        {
            var slNow = DateTimeHelper.Now;
            var slToday = DateTimeHelper.Today;
            var tz = DateTimeHelper.TimeZone;

            Assert.NotNull(tz);
            // Sri Lanka offset is always +05:30 (330 minutes)
            Assert.Equal(TimeSpan.FromMinutes(330), tz.BaseUtcOffset);

            // Converting a known UTC time 02:30 should yield 08:00 SL time
            var utcTime = new DateTime(2026, 10, 21, 2, 30, 0, DateTimeKind.Utc);
            var converted = DateTimeHelper.ToSriLankaTime(utcTime);

            Assert.Equal(8, converted.Hour);
            Assert.Equal(0, converted.Minute);
            Assert.Equal(2026, converted.Year);
            Assert.Equal(10, converted.Month);
            Assert.Equal(21, converted.Day);
        }

        [Fact]
        public void CheckInRequestDto_Accepts_Optional_DeviceTime()
        {
            var dto = new CheckInRequestDto
            {
                Latitude = 6.9271m,
                Longitude = 79.8612m,
                DeviceTime = new DateTime(2026, 10, 21, 8, 30, 0)
            };

            Assert.NotNull(dto.DeviceTime);
            Assert.Equal(8, dto.DeviceTime.Value.Hour);
            Assert.Equal(30, dto.DeviceTime.Value.Minute);
        }

        [Fact]
        public void Employee_Entity_And_Dtos_Support_BankDetails_And_Qualification()
        {
            var emp = new Employee
            {
                Id = 10,
                CompanyId = 1,
                BranchId = 2,
                EmployeeCode = "EMP999",
                FirstName = "Test",
                LastName = "User",
                JoinedDate = new DateOnly(2026, 1, 1),
                IsActive = true,
                BankName = "Commercial Bank",
                BankAccountNo = "8001234567",
                BankBranch = "Colombo Fort",
                Qualification = "B.Sc in Software Engineering"
            };

            Assert.Equal("Commercial Bank", emp.BankName);
            Assert.Equal("8001234567", emp.BankAccountNo);
            Assert.Equal("Colombo Fort", emp.BankBranch);
            Assert.Equal("B.Sc in Software Engineering", emp.Qualification);

            var createDto = new CreateEmployeeDto
            {
                BankName = "Bank of Ceylon",
                BankAccountNo = "1234567890",
                BankBranch = "Kandy",
                Qualification = "MBA"
            };

            Assert.Equal("Bank of Ceylon", createDto.BankName);
            Assert.Equal("1234567890", createDto.BankAccountNo);
            Assert.Equal("Kandy", createDto.BankBranch);
            Assert.Equal("MBA", createDto.Qualification);

            var updateDto = new UpdateEmployeeDto
            {
                BankName = "Sampath Bank",
                BankAccountNo = "9876543210",
                BankBranch = "Galle",
                Qualification = "M.Sc"
            };

            Assert.Equal("Sampath Bank", updateDto.BankName);
            Assert.Equal("9876543210", updateDto.BankAccountNo);
            Assert.Equal("Galle", updateDto.BankBranch);
            Assert.Equal("M.Sc", updateDto.Qualification);

            var empDto = new EmployeeDto
            {
                BankName = "Hatton National Bank",
                BankAccountNo = "5555555555",
                BankBranch = "Bandarawela",
                Qualification = "B.Sc"
            };

            Assert.Equal("Hatton National Bank", empDto.BankName);
            Assert.Equal("5555555555", empDto.BankAccountNo);
            Assert.Equal("Bandarawela", empDto.BankBranch);
            Assert.Equal("B.Sc", empDto.Qualification);
        }

        [Fact]
        public void LoginResponse_Supports_Dynamic_BranchName()
        {
            var response = new LoginResponse
            {
                UserId = 1,
                EmployeeId = 5,
                Username = "emp1",
                BranchId = 3,
                BranchName = "Bandarawela Branch"
            };

            Assert.Equal(3, response.BranchId);
            Assert.Equal("Bandarawela Branch", response.BranchName);
        }

        [Fact]
        public void Shift_Punctuality_And_OT_Calculations_Matches_830_To_1930()
        {
            var normalStart = new TimeOnly(8, 30);
            var normalEnd = new TimeOnly(19, 30);

            // On-time check-in at 08:20
            var checkInOnTime = new TimeOnly(8, 20);
            var isLateOnTime = checkInOnTime > normalStart;
            Assert.False(isLateOnTime);

            // Late check-in at 08:45 -> 15 minutes late
            var checkInLate = new TimeOnly(8, 45);
            var isLate = checkInLate > normalStart;
            var lateMinutes = (int)(checkInLate.ToTimeSpan() - normalStart.ToTimeSpan()).TotalMinutes;
            Assert.True(isLate);
            Assert.Equal(15, lateMinutes);

            // Early check-out at 18:30 -> 60 minutes early, OT = 0
            var checkOutEarly = new TimeOnly(18, 30);
            var isEarly = checkOutEarly < normalEnd;
            var earlyMinutes = (int)(normalEnd.ToTimeSpan() - checkOutEarly.ToTimeSpan()).TotalMinutes;
            Assert.True(isEarly);
            Assert.Equal(60, earlyMinutes);

            // Overtime check-out at 20:45 (8:45 PM) -> OT = 75 minutes, EarlyDeparture = false
            var checkOutOT = new TimeOnly(20, 45);
            var isEarlyOT = checkOutOT < normalEnd;
            var otMinutes = (int)(checkOutOT.ToTimeSpan() - normalEnd.ToTimeSpan()).TotalMinutes;
            Assert.False(isEarlyOT);
            Assert.Equal(75, otMinutes);
        }
    }
}
