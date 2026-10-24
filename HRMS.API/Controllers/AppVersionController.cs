using System;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.Data;
using HRMS.API.DTOs.AppVersion;
using HRMS.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppVersionController : ControllerBase
    {
        private readonly DefaultContext _context;

        public AppVersionController(DefaultContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Check if the client application requires an update
        /// </summary>
        [HttpGet("check")]
        [AllowAnonymous]
        public async Task<IActionResult> Check(
            [FromQuery] string platform = "Android",
            [FromQuery] string? currentVersion = null)
        {
            try
            {
                var normPlatform = (platform ?? "Android").Trim();
                var appVersion = await _context.AppVersions
                    .FirstOrDefaultAsync(v => v.Platform.ToLower() == normPlatform.ToLower());

                if (appVersion == null)
                {
                    // Fallback to default
                    appVersion = new AppVersion
                    {
                        Platform = normPlatform,
                        LatestVersion = "1.0.3",
                        MinimumVersion = "1.0.3",
                        ForceUpdate = true,
                        ReleaseNotes = "Official AlienHR Enterprise v1.0.3 release."
                    };
                }

                var normalizedCurrent = (currentVersion ?? string.Empty).Trim();
                bool isUpdateRequired = false;
                string message = "Application is up to date.";

                if (string.IsNullOrWhiteSpace(normalizedCurrent))
                {
                    isUpdateRequired = true;
                    message = $"An update is required. Please install AlienHR Enterprise v{appVersion.LatestVersion}.";
                }
                else
                {
                    isUpdateRequired = CheckIfUpdateRequired(normalizedCurrent, appVersion.MinimumVersion, appVersion.LatestVersion, appVersion.ForceUpdate);
                    if (isUpdateRequired)
                    {
                        message = $"Your app version (v{normalizedCurrent}) is outdated. Minimum required version is v{appVersion.MinimumVersion}. Please update to continue.";
                    }
                }

                var response = new AppVersionCheckResponseDto
                {
                    Platform = appVersion.Platform,
                    LatestVersion = appVersion.LatestVersion,
                    MinimumVersion = appVersion.MinimumVersion,
                    CurrentVersion = normalizedCurrent,
                    IsUpdateRequired = isUpdateRequired,
                    ForceUpdate = appVersion.ForceUpdate,
                    UpdateUrl = appVersion.UpdateUrl,
                    ReleaseNotes = appVersion.ReleaseNotes,
                    Message = message
                };

                return Ok(new
                {
                    success = true,
                    data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Version check failed: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get latest version config for a platform
        /// </summary>
        [HttpGet("latest")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLatest([FromQuery] string platform = "Android")
        {
            var normPlatform = (platform ?? "Android").Trim();
            var appVersion = await _context.AppVersions
                .FirstOrDefaultAsync(v => v.Platform.ToLower() == normPlatform.ToLower());

            if (appVersion == null)
            {
                appVersion = new AppVersion
                {
                    Platform = normPlatform,
                    LatestVersion = "1.0.3",
                    MinimumVersion = "1.0.3",
                    ForceUpdate = true
                };
            }

            return Ok(new
            {
                success = true,
                data = appVersion
            });
        }

        /// <summary>
        /// Get all platform version records
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.AppVersions.ToListAsync();
            return Ok(new
            {
                success = true,
                data = list
            });
        }

        /// <summary>
        /// Update required version configuration in database (Admin)
        /// </summary>
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateAppVersionDto request)
        {
            var normPlatform = (request.Platform ?? "Android").Trim();
            var existing = await _context.AppVersions
                .FirstOrDefaultAsync(v => v.Platform.ToLower() == normPlatform.ToLower());

            if (existing == null)
            {
                existing = new AppVersion
                {
                    Platform = normPlatform
                };
                await _context.AppVersions.AddAsync(existing);
            }

            existing.LatestVersion = request.LatestVersion.Trim();
            existing.MinimumVersion = request.MinimumVersion.Trim();
            existing.ForceUpdate = request.ForceUpdate;
            existing.UpdateUrl = request.UpdateUrl;
            existing.ReleaseNotes = request.ReleaseNotes;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"App version configuration updated for {normPlatform}.",
                data = existing
            });
        }

        private static bool CheckIfUpdateRequired(string currentVer, string minVer, string latestVer, bool forceUpdate)
        {
            if (string.Equals(currentVer, latestVer, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Parse semantic versions
            if (TryParseVersion(currentVer, out var current) && TryParseVersion(minVer, out var min))
            {
                if (current < min)
                {
                    return true;
                }
            }
            else
            {
                // Fallback to strict string inequality
                if (!string.Equals(currentVer, minVer, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            if (forceUpdate && !string.Equals(currentVer, latestVer, StringComparison.OrdinalIgnoreCase))
            {
                if (TryParseVersion(currentVer, out var curr) && TryParseVersion(latestVer, out var latest))
                {
                    return curr < latest;
                }
                return true;
            }

            return false;
        }

        private static bool TryParseVersion(string v, out Version version)
        {
            // Strip any leading 'v' or trailing build numbers (e.g. "v1.0.3+4" -> "1.0.3")
            var cleaned = v.TrimStart('v', 'V');
            var plusIdx = cleaned.IndexOf('+');
            if (plusIdx >= 0)
            {
                cleaned = cleaned.Substring(0, plusIdx);
            }

            var parts = cleaned.Split('.');
            if (parts.Length == 2)
            {
                cleaned += ".0";
            }

            return Version.TryParse(cleaned, out version!);
        }
    }
}
