using System;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.Data;
using HRMS.API.DTOs.Notice;
using HRMS.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoticeController : ControllerBase
    {
        private readonly DefaultContext _context;

        public NoticeController(DefaultContext context)
        {
            _context = context;
        }

        // GET: api/Notice/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveNotice([FromQuery] string? role = null)
        {
            try
            {
                var query = _context.SystemNotices
                    .Where(n => n.IsActive);

                if (!string.IsNullOrEmpty(role) && role != "All")
                {
                    query = query.Where(n => n.TargetRole == "All" || n.TargetRole.ToLower() == role.ToLower());
                }

                var notice = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new NoticeDto
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Message = n.Message,
                        ImageUrl = n.ImageUrl,
                        IsActive = n.IsActive,
                        TargetRole = n.TargetRole,
                        CreatedAt = n.CreatedAt,
                        CreatedBy = n.CreatedBy
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    data = notice
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // GET: api/Notice
        [HttpGet]
        public async Task<IActionResult> GetAllNotices()
        {
            try
            {
                var list = await _context.SystemNotices
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new NoticeDto
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Message = n.Message,
                        ImageUrl = n.ImageUrl,
                        IsActive = n.IsActive,
                        TargetRole = n.TargetRole,
                        CreatedAt = n.CreatedAt,
                        CreatedBy = n.CreatedBy
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = list
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // POST: api/Notice
        [HttpPost]
        public async Task<IActionResult> CreateNotice([FromBody] CreateNoticeDto dto, [FromQuery] int? createdBy = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Title))
                {
                    return BadRequest(new { success = false, message = "Notice title is required." });
                }

                // If this new notice is active, we can optionally deactivate others or keep them
                var notice = new SystemNotice
                {
                    Title = dto.Title.Trim(),
                    Message = dto.Message?.Trim(),
                    ImageUrl = dto.ImageUrl?.Trim(),
                    IsActive = dto.IsActive,
                    TargetRole = string.IsNullOrWhiteSpace(dto.TargetRole) ? "All" : dto.TargetRole.Trim(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = createdBy
                };

                await _context.SystemNotices.AddAsync(notice);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "System notice poster created successfully.",
                    data = new NoticeDto
                    {
                        Id = notice.Id,
                        Title = notice.Title,
                        Message = notice.Message,
                        ImageUrl = notice.ImageUrl,
                        IsActive = notice.IsActive,
                        TargetRole = notice.TargetRole,
                        CreatedAt = notice.CreatedAt,
                        CreatedBy = notice.CreatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // PUT: api/Notice/{id}/toggle
        [HttpPut("{id:long}/toggle")]
        public async Task<IActionResult> ToggleNotice(long id)
        {
            try
            {
                var notice = await _context.SystemNotices.FindAsync(id);
                if (notice == null)
                {
                    return NotFound(new { success = false, message = "Notice not found." });
                }

                notice.IsActive = !notice.IsActive;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Notice is now {(notice.IsActive ? "Active" : "Inactive")}.",
                    data = notice.IsActive
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // DELETE: api/Notice/{id}
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteNotice(long id)
        {
            try
            {
                var notice = await _context.SystemNotices.FindAsync(id);
                if (notice == null)
                {
                    return NotFound(new { success = false, message = "Notice not found." });
                }

                _context.SystemNotices.Remove(notice);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Notice deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
