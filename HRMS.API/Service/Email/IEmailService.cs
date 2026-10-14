using System;
using System.Threading.Tasks;

namespace HRMS.API.Service.Email
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(
            string recipientEmail,
            string recipientName,
            string subject,
            string htmlBody,
            string? plainTextBody = null);

        Task<bool> SendLeaveDecisionEmailAsync(
            string recipientEmail,
            string employeeName,
            string leaveType,
            DateOnly fromDate,
            DateOnly toDate,
            decimal totalDays,
            bool isApproved,
            string? reviewerComments);

        Task<bool> SendAttendanceCorrectionDecisionEmailAsync(
            string recipientEmail,
            string employeeName,
            DateOnly attendanceDate,
            bool isApproved,
            string? reviewerComments);

        Task<bool> SendPayslipPublishedEmailAsync(
            string recipientEmail,
            string employeeName,
            string periodName,
            decimal netSalary);
    }
}
