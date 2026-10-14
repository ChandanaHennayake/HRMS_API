using System;

namespace HRMS.API.DTOs.Notification
{
    public class NotificationDto
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public long? EmployeeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Info";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
    }

    public class CreateNotificationDto
    {
        public int? UserId { get; set; }
        public long? EmployeeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Info";
        public string? ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
    }

    public class NotificationSummaryDto
    {
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
    }
}
