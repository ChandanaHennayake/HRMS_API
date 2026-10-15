using System;

namespace HRMS.API.DTOs.Notice
{
    public class NoticeDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public string TargetRole { get; set; } = "All";
        public string TargetAudience => TargetRole;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
    }

    public class CreateNoticeDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public string TargetRole { get; set; } = "All";
        public string? TargetAudience { get; set; }
    }
}
