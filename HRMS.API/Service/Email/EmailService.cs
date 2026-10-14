using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using HRMS.API.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HRMS.API.Service.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> settings,
            ILogger<EmailService> logger)
        {
            _settings = settings?.Value ?? new EmailSettings();
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(
            string recipientEmail,
            string recipientName,
            string subject,
            string htmlBody,
            string? plainTextBody = null)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                _logger.LogWarning("Email sending skipped: recipient email address is empty.");
                return false;
            }

            // MOCK MODE (Local development & CI tests)
            if (_settings.UseMock || string.IsNullOrWhiteSpace(_settings.Username))
            {
                _logger.LogInformation(
                    "[MOCK EMAIL GATEWAY] To: {Name} <{Email}> | Subject: '{Subject}' | Body Preview: {Preview}",
                    recipientName,
                    recipientEmail,
                    subject,
                    plainTextBody ?? (htmlBody.Length > 100 ? htmlBody.Substring(0, 100) + "..." : htmlBody));
                return true;
            }

            // LIVE SMTP DISPATCH
            try
            {
                using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
                {
                    EnableSsl = _settings.EnableSsl,
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password)
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(new MailAddress(recipientEmail, recipientName));

                if (!string.IsNullOrWhiteSpace(plainTextBody))
                {
                    var plainView = AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain");
                    mailMessage.AlternateViews.Add(plainView);
                }

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email successfully dispatched to {Recipient} ({Subject})", recipientEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient} via SMTP: {Message}", recipientEmail, ex.Message);
                return false;
            }
        }

        public async Task<bool> SendLeaveDecisionEmailAsync(
            string recipientEmail,
            string employeeName,
            string leaveType,
            DateOnly fromDate,
            DateOnly toDate,
            decimal totalDays,
            bool isApproved,
            string? reviewerComments)
        {
            var statusText = isApproved ? "APPROVED" : "REJECTED";
            var statusColor = isApproved ? "#059669" : "#DC2626";
            var subject = $"Leave Request {statusText}: {leaveType} ({fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd})";

            var html = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #E2E8F0; border-radius: 8px;'>
                    <h2 style='color: #6B21A8; margin-bottom: 8px;'>AlienHR Notification</h2>
                    <p>Dear {employeeName},</p>
                    <p>Your leave request has been <strong style='color: {statusColor};'>{statusText}</strong>.</p>
                    <table style='width: 100%; border-collapse: collapse; margin: 16px 0;'>
                        <tr><td style='padding: 8px; color: #64748B;'>Leave Type:</td><td style='padding: 8px; font-weight: bold;'>{leaveType}</td></tr>
                        <tr><td style='padding: 8px; color: #64748B;'>Duration:</td><td style='padding: 8px;'>{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd} ({totalDays} days)</td></tr>
                        {(string.IsNullOrWhiteSpace(reviewerComments) ? "" : $"<tr><td style='padding: 8px; color: #64748B;'>Manager Remarks:</td><td style='padding: 8px; font-style: italic;'>{reviewerComments}</td></tr>")}
                    </table>
                    <p style='color: #64748B; font-size: 12px; margin-top: 24px;'>This is an automated notification from AlienHR. Please check your mobile portal for updated balances.</p>
                </div>";

            return await SendEmailAsync(recipientEmail, employeeName, subject, html);
        }

        public async Task<bool> SendAttendanceCorrectionDecisionEmailAsync(
            string recipientEmail,
            string employeeName,
            DateOnly attendanceDate,
            bool isApproved,
            string? reviewerComments)
        {
            var statusText = isApproved ? "APPROVED" : "REJECTED";
            var statusColor = isApproved ? "#059669" : "#DC2626";
            var subject = $"Attendance Correction {statusText}: {attendanceDate:yyyy-MM-dd}";

            var html = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #E2E8F0; border-radius: 8px;'>
                    <h2 style='color: #6B21A8; margin-bottom: 8px;'>AlienHR Attendance Correction</h2>
                    <p>Dear {employeeName},</p>
                    <p>Your attendance correction request for <strong>{attendanceDate:yyyy-MM-dd}</strong> has been <strong style='color: {statusColor};'>{statusText}</strong>.</p>
                    {(string.IsNullOrWhiteSpace(reviewerComments) ? "" : $"<p><strong>Reviewer Comment:</strong> {reviewerComments}</p>")}
                    <p style='color: #64748B; font-size: 12px; margin-top: 24px;'>Log in to your mobile app to view your updated attendance record.</p>
                </div>";

            return await SendEmailAsync(recipientEmail, employeeName, subject, html);
        }

        public async Task<bool> SendPayslipPublishedEmailAsync(
            string recipientEmail,
            string employeeName,
            string periodName,
            decimal netSalary)
        {
            var subject = $"Payslip Published: {periodName}";

            var html = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #E2E8F0; border-radius: 8px;'>
                    <h2 style='color: #6B21A8; margin-bottom: 8px;'>Payslip Published</h2>
                    <p>Dear {employeeName},</p>
                    <p>Your payslip for <strong>{periodName}</strong> has been processed and published.</p>
                    <div style='background-color: #F5F3FF; padding: 16px; border-radius: 8px; margin: 16px 0; border: 1px solid #DDD6FE;'>
                        <span style='color: #6B21A8; font-size: 12px; font-weight: bold;'>NET SALARY PAYOUT</span>
                        <div style='font-size: 22px; font-weight: bold; color: #1E1B4B; margin-top: 4px;'>LKR {netSalary:N2}</div>
                    </div>
                    <p style='color: #64748B; font-size: 12px; margin-top: 24px;'>You can view, verify, and download your detailed itemized payslip in your AlienHR mobile app under <em>Profile &gt; Payslips</em>.</p>
                </div>";

            return await SendEmailAsync(recipientEmail, employeeName, subject, html);
        }
    }
}
