using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HRMS.API.DTOs.ProfileChange;
using HRMS.API.Service.ProfileChange;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileChangeController : ControllerBase
    {
        private readonly IProfileChangeService _profileChangeService;

        public ProfileChangeController(IProfileChangeService profileChangeService)
        {
            _profileChangeService = profileChangeService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("UserId")?.Value
                ?? User.FindFirst("id")?.Value;

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return 0;
        }

        // =====================================================
        // SUBMIT PROFILE CHANGE REQUEST
        // POST: api/ProfileChange
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitProfileChangeRequestDto request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _profileChangeService.SubmitRequestAsync(request, userId);
                return Ok(new
                {
                    success = true,
                    message = "Profile change request submitted successfully and is awaiting review.",
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
        // GET EMPLOYEE'S OWN REQUESTS
        // GET: api/ProfileChange/my?employeeId=5
        // =====================================================
        [HttpGet("my")]
        public async Task<IActionResult> GetMyRequests([FromQuery] long employeeId = 0)
        {
            try
            {
                int userId = GetCurrentUserId();
                var list = await _profileChangeService.GetMyRequestsAsync(employeeId, userId);
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

        // =====================================================
        // GET ALL REQUESTS (ADMIN & MANAGER)
        // GET: api/ProfileChange?companyId=1&branchId=2&status=0
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] long? companyId = null,
            [FromQuery] long? branchId = null,
            [FromQuery] short? status = null)
        {
            try
            {
                var list = await _profileChangeService.GetAllRequestsAsync(companyId, branchId, status);
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

        // =====================================================
        // GET REQUEST DETAILS BY ID
        // GET: api/ProfileChange/1
        // =====================================================
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var item = await _profileChangeService.GetByIdAsync(id);
                if (item == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Profile change request not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = item
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
        // APPROVE PROFILE CHANGE REQUEST
        // POST: api/ProfileChange/1/approve
        // =====================================================
        [HttpPost("{id:long}/approve")]
        public async Task<IActionResult> Approve(long id, [FromBody] ReviewProfileChangeDto reviewDto)
        {
            try
            {
                int reviewerId = reviewDto.ReviewerUserId.HasValue && reviewDto.ReviewerUserId.Value > 0
                    ? (int)reviewDto.ReviewerUserId.Value
                    : GetCurrentUserId();

                if (reviewerId <= 0) reviewerId = 1; // Default to administrator if not authenticated in context

                var result = await _profileChangeService.ApproveRequestAsync(id, reviewDto, reviewerId);
                return Ok(new
                {
                    success = true,
                    message = "Profile change request approved successfully. Employee and user records updated.",
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
        // REJECT PROFILE CHANGE REQUEST
        // POST: api/ProfileChange/1/reject
        // =====================================================
        [HttpPost("{id:long}/reject")]
        public async Task<IActionResult> Reject(long id, [FromBody] ReviewProfileChangeDto reviewDto)
        {
            try
            {
                int reviewerId = reviewDto.ReviewerUserId.HasValue && reviewDto.ReviewerUserId.Value > 0
                    ? (int)reviewDto.ReviewerUserId.Value
                    : GetCurrentUserId();

                if (reviewerId <= 0) reviewerId = 1;

                var result = await _profileChangeService.RejectRequestAsync(id, reviewDto, reviewerId);
                return Ok(new
                {
                    success = true,
                    message = "Profile change request rejected.",
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
        // CANCEL PROFILE CHANGE REQUEST
        // POST: api/ProfileChange/1/cancel
        // =====================================================
        [HttpPost("{id:long}/cancel")]
        public async Task<IActionResult> Cancel(long id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var success = await _profileChangeService.CancelRequestAsync(id, userId);
                if (!success)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Request not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Profile change request cancelled."
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
