using System;
using System.Threading.Tasks;
using HRMS.API.DTOs.Audit;
using HRMS.API.Service.Audit;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditLogController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        // =====================================================
        // GET PAGED AUDIT LOGS
        // GET: api/AuditLog?page=1&pageSize=30&action=...
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogQueryDto query)
        {
            try
            {
                var result = await _auditService.GetPagedAsync(query);
                return Ok(new
                {
                    success = true,
                    data = result
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

        // =====================================================
        // GET AUDIT STATS
        // GET: api/AuditLog/stats
        // =====================================================
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _auditService.GetStatsAsync();
                return Ok(new
                {
                    success = true,
                    data = stats
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

        // =====================================================
        // GET DISTINCT ACTIONS
        // GET: api/AuditLog/actions
        // =====================================================
        [HttpGet("actions")]
        public async Task<IActionResult> GetActions()
        {
            try
            {
                var actions = await _auditService.GetActionsAsync();
                return Ok(new
                {
                    success = true,
                    data = actions
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

        // =====================================================
        // GET DISTINCT ENTITIES
        // GET: api/AuditLog/entities
        // =====================================================
        [HttpGet("entities")]
        public async Task<IActionResult> GetEntities()
        {
            try
            {
                var entities = await _auditService.GetEntitiesAsync();
                return Ok(new
                {
                    success = true,
                    data = entities
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

        // =====================================================
        // GET AUDIT LOG BY ID
        // GET: api/AuditLog/123
        // =====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var log = await _auditService.GetByIdAsync(id);
                if (log == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Audit log record not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = log
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
