using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.DTOs.Notification;
using HRMS.API.Entities;
using HRMS.API.Repository.Notification;
using Microsoft.Extensions.Logging;

namespace HRMS.API.Service.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(INotificationRepository repository, ILogger<NotificationService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<NotificationDto> SendNotificationAsync(CreateNotificationDto dto)
        {
            int targetUserId = dto.UserId ?? 0;

            if (targetUserId == 0 && dto.EmployeeId.HasValue)
            {
                var resolvedId = await _repository.FindUserIdByEmployeeIdAsync(dto.EmployeeId.Value);
                if (resolvedId.HasValue)
                {
                    targetUserId = resolvedId.Value;
                }
            }

            if (targetUserId == 0)
            {
                _logger.LogWarning("Cannot send notification: No valid UserId found for EmployeeId {EmployeeId}", dto.EmployeeId);
                return new NotificationDto
                {
                    Title = dto.Title,
                    Message = dto.Message,
                    Type = dto.Type
                };
            }

            var entity = new Entities.Notification
            {
                UserId = targetUserId,
                EmployeeId = dto.EmployeeId,
                Title = dto.Title,
                Message = dto.Message,
                Type = string.IsNullOrWhiteSpace(dto.Type) ? "Info" : dto.Type,
                IsRead = false,
                ReferenceType = dto.ReferenceType,
                ReferenceId = dto.ReferenceId
            };

            var created = await _repository.CreateAsync(entity);
            return MapToDto(created);
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, int limit = 50)
        {
            var list = await _repository.GetByUserIdAsync(userId, limit);
            return list.Select(MapToDto);
        }

        public async Task<NotificationSummaryDto> GetSummaryAsync(int userId)
        {
            var unread = await _repository.GetUnreadCountAsync(userId);
            var list = await _repository.GetByUserIdAsync(userId, 100);
            return new NotificationSummaryDto
            {
                UnreadCount = unread,
                TotalCount = list.Count()
            };
        }

        public async Task MarkAsReadAsync(long notificationId)
        {
            await _repository.MarkAsReadAsync(notificationId);
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _repository.MarkAllAsReadAsync(userId);
        }

        public async Task NotifyLeaveDecisionAsync(long employeeId, string leaveTypeName, bool isApproved, string? comment)
        {
            try
            {
                var dto = new CreateNotificationDto
                {
                    EmployeeId = employeeId,
                    Title = isApproved ? "Leave Request Approved" : "Leave Request Rejected",
                    Message = isApproved
                        ? $"Your request for {leaveTypeName} has been approved."
                        : $"Your request for {leaveTypeName} was rejected. Reason: {comment ?? "No reason provided."}",
                    Type = isApproved ? "Success" : "Warning",
                    ReferenceType = "Leave"
                };
                await SendNotificationAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch leave decision notification for employee {EmployeeId}", employeeId);
            }
        }

        public async Task NotifyAttendanceCorrectionDecisionAsync(long employeeId, DateOnly attendanceDate, bool isApproved, string? comment)
        {
            try
            {
                var dto = new CreateNotificationDto
                {
                    EmployeeId = employeeId,
                    Title = isApproved ? "Attendance Correction Approved" : "Attendance Correction Rejected",
                    Message = isApproved
                        ? $"Your attendance correction for {attendanceDate:yyyy-MM-dd} has been approved."
                        : $"Your attendance correction for {attendanceDate:yyyy-MM-dd} was rejected. Reason: {comment ?? "No reason provided."}",
                    Type = isApproved ? "Success" : "Warning",
                    ReferenceType = "AttendanceCorrection"
                };
                await SendNotificationAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch attendance correction notification for employee {EmployeeId}", employeeId);
            }
        }

        public async Task NotifyPayrollPublishedAsync(long employeeId, string periodName, decimal netSalary, long payslipId)
        {
            try
            {
                var dto = new CreateNotificationDto
                {
                    EmployeeId = employeeId,
                    Title = "Payslip Published",
                    Message = $"Your payslip for {periodName} is now available. Net Pay: {netSalary:N2} LKR.",
                    Type = "Info",
                    ReferenceType = "Payslip",
                    ReferenceId = payslipId
                };
                await SendNotificationAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch payslip published notification for employee {EmployeeId}", employeeId);
            }
        }

        private static NotificationDto MapToDto(Entities.Notification n)
        {
            return new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                EmployeeId = n.EmployeeId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt,
                ReferenceType = n.ReferenceType,
                ReferenceId = n.ReferenceId
            };
        }
    }
}
