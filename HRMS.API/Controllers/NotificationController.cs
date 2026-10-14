using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HRMS.API.DTOs.Notification;
using HRMS.API.Service.Notification;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // =====================================================
        // GET USER NOTIFICATIONS
        // GET: api/Notification/my?userId=1&limit=50
        // =====================================================
        [HttpGet("my")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int? userId = null, [FromQuery] int limit = 50)
        {
            try
            {
                var targetUserId = ResolveUserId(userId);
                if (targetUserId == 0)
                {
                    return BadRequest(new { success = false, message = "UserId must be provided or available in claims." });
                }

                var list = await _notificationService.GetUserNotificationsAsync(targetUserId, limit);
                return Ok(new
                {
                    success = true,
                    data = list
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // =====================================================
        // GET SUMMARY (UNREAD COUNT)
        // GET: api/Notification/summary?userId=1
        // =====================================================
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int? userId = null)
        {
            try
            {
                var targetUserId = ResolveUserId(userId);
                if (targetUserId == 0)
                {
                    return BadRequest(new { success = false, message = "UserId must be provided or available in claims." });
                }

                var summary = await _notificationService.GetSummaryAsync(targetUserId);
                return Ok(new
                {
                    success = true,
                    data = summary
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // =====================================================
        // MARK A NOTIFICATION AS READ
        // PUT: api/Notification/123/read
        // =====================================================
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(id);
                return Ok(new
                {
                    success = true,
                    message = "Notification marked as read."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // =====================================================
        // MARK ALL AS READ
        // PUT: api/Notification/read-all?userId=1
        // =====================================================
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] int? userId = null)
        {
            try
            {
                var targetUserId = ResolveUserId(userId);
                if (targetUserId == 0)
                {
                    return BadRequest(new { success = false, message = "UserId must be provided or available in claims." });
                }

                await _notificationService.MarkAllAsReadAsync(targetUserId);
                return Ok(new
                {
                    success = true,
                    message = "All notifications marked as read."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // =====================================================
        // SEND NOTIFICATION (CUSTOM / MANUAL)
        // POST: api/Notification
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> SendNotification([FromBody] CreateNotificationDto dto)
        {
            try
            {
                var result = await _notificationService.SendNotificationAsync(dto);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private int ResolveUserId(int? queryUserId)
        {
            if (queryUserId.HasValue && queryUserId.Value > 0)
            {
                return queryUserId.Value;
            }

            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? User.FindFirst("UserId")?.Value;

            if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out int parsed))
            {
                return parsed;
            }

            return 0;
        }
    }
}
