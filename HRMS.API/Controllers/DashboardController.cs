using HRMS.API.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get high-level organization attendance and employee metrics for Admin.
        /// Optional companyId parameter scopes to specific company.
        /// </summary>
        [HttpGet("admin-summary")]
        public async Task<IActionResult> GetAdminSummary([FromQuery] long? companyId)
        {
            try
            {
                var summary = await _dashboardService.GetAdminSummaryAsync(companyId);
                return Ok(new
                {
                    success = true,
                    data = summary
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

        /// <summary>
        /// Get team metrics, leave list, and weekly attendance for Manager by branch.
        /// </summary>
        [HttpGet("manager-summary")]
        public async Task<IActionResult> GetManagerSummary([FromQuery] long branchId)
        {
            try
            {
                if (branchId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Valid branchId must be provided."
                    });
                }

                var summary = await _dashboardService.GetManagerSummaryAsync(branchId);
                return Ok(new
                {
                    success = true,
                    data = summary
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
