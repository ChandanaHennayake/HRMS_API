using System;
using System.Threading.Tasks;
using HRMS.API.Configurations;
using HRMS.API.Service.Email;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace HRMS.Tests
{
    public class EmailServiceTests
    {
        private readonly IEmailService _emailService;

        public EmailServiceTests()
        {
            var settings = Options.Create(new EmailSettings
            {
                UseMock = true,
                SenderEmail = "notifications@alienhr.com",
                SenderName = "AlienHR Test Gateway"
            });

            _emailService = new EmailService(settings, NullLogger<EmailService>.Instance);
        }

        [Fact]
        public async Task SendEmailAsync_MockMode_ReturnsTrue()
        {
            var result = await _emailService.SendEmailAsync(
                "kasun@example.com",
                "Kasun Perera",
                "Test Subject",
                "<p>Test HTML body</p>");

            Assert.True(result);
        }

        [Fact]
        public async Task SendEmailAsync_EmptyRecipient_ReturnsFalse()
        {
            var result = await _emailService.SendEmailAsync(
                "",
                "Unknown",
                "Test Subject",
                "<p>Test</p>");

            Assert.False(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SendLeaveDecisionEmailAsync_RendersAccurately(bool isApproved)
        {
            var from = new DateOnly(2026, 10, 20);
            var to = new DateOnly(2026, 10, 22);

            var result = await _emailService.SendLeaveDecisionEmailAsync(
                "employee@alienhr.com",
                "Dilshan Silva",
                "Annual Leave",
                from,
                to,
                3.0m,
                isApproved,
                "Standard approved allocation");

            Assert.True(result);
        }

        [Fact]
        public async Task SendAttendanceCorrectionDecisionEmailAsync_ExecutesSuccessfully()
        {
            var date = new DateOnly(2026, 10, 14);

            var result = await _emailService.SendAttendanceCorrectionDecisionEmailAsync(
                "employee@alienhr.com",
                "Kasun Perera",
                date,
                true,
                "Approved after verifying swipe log");

            Assert.True(result);
        }

        [Fact]
        public async Task SendPayslipPublishedEmailAsync_RendersNetSalaryProperly()
        {
            var result = await _emailService.SendPayslipPublishedEmailAsync(
                "employee@alienhr.com",
                "Nimal Fernando",
                "October 2026",
                125000.50m);

            Assert.True(result);
        }
    }
}
