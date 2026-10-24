using System;

namespace HRMS.API.DTOs.AppVersion
{
    public class AppVersionCheckResponseDto
    {
        public string Platform { get; set; } = "Android";
        public string LatestVersion { get; set; } = "1.0.3";
        public string MinimumVersion { get; set; } = "1.0.3";
        public string CurrentVersion { get; set; } = string.Empty;
        public bool IsUpdateRequired { get; set; }
        public bool ForceUpdate { get; set; } = true;
        public string? UpdateUrl { get; set; }
        public string? ReleaseNotes { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class UpdateAppVersionDto
    {
        public string Platform { get; set; } = "Android";
        public string LatestVersion { get; set; } = "1.0.3";
        public string MinimumVersion { get; set; } = "1.0.3";
        public bool ForceUpdate { get; set; } = true;
        public string? UpdateUrl { get; set; }
        public string? ReleaseNotes { get; set; }
    }
}
