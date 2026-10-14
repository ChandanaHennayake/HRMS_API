using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.API.DTOs.Notification;

namespace HRMS.API.Service.Notification
{
    public interface INotificationService
    {
        Task<NotificationDto> SendNotificationAsync(CreateNotificationDto dto);
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, int limit = 50);
        Task<NotificationSummaryDto> GetSummaryAsync(int userId);
        Task MarkAsReadAsync(long notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task NotifyLeaveDecisionAsync(long employeeId, string leaveTypeName, bool isApproved, string? comment);
        Task NotifyAttendanceCorrectionDecisionAsync(long employeeId, DateOnly attendanceDate, bool isApproved, string? comment);
        Task NotifyPayrollPublishedAsync(long employeeId, string periodName, decimal netSalary, long payslipId);
    }
}
