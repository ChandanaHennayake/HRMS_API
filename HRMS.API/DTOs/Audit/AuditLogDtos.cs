using System;
using System.Collections.Generic;

namespace HRMS.API.DTOs.Audit
{
    public class AuditLogDto
    {
        public long Id { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? Description { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string Severity { get; set; } = "Info";
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAuditLogDto
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? Description { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string Severity { get; set; } = "Info";
    }

    public class AuditLogQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 30;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Action { get; set; }
        public string? EntityName { get; set; }
        public int? UserId { get; set; }
        public string? Severity { get; set; }
        public string? Search { get; set; }
    }

    public class PagedAuditResultDto
    {
        public IEnumerable<AuditLogDto> Items { get; set; } = new List<AuditLogDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }

    public class AuditStatsDto
    {
        public int TotalLogs { get; set; }
        public int SecurityCount { get; set; }
        public int WarningCount { get; set; }
        public int CriticalCount { get; set; }
        public int InfoCount { get; set; }
        public Dictionary<string, int> TopActions { get; set; } = new();
    }
}
