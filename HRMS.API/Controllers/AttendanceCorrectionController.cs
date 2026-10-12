using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HRMS.API.DTOs.Attendance;
using HRMS.API.Service.Attendance;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceCorrectionController : ControllerBase
    {
        private readonly IAttendanceCorrectionService _correctionService;

        public AttendanceCorrectionController(IAttendanceCorrectionService correctionService)
        {
            _correctionService = correctionService;
        }

        // =====================================================
        // SUBMIT CORRECTION REQUEST
        // POST: api/AttendanceCorrection/request?employeeId=5
        // =====================================================
        [HttpPost("request")]
        public async Task<IActionResult> SubmitRequest(
            [FromQuery] long employeeId,
            [FromBody] CreateAttendanceCorrectionDto request)
        {
            try
            {
                var result = await _correctionService.SubmitCorrectionAsync(employeeId, request);
                return Ok(new
                {
                    success = true,
                    message = "Attendance correction request submitted successfully.",
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
        // GET EMPLOYEE CORRECTIONS
        // GET: api/AttendanceCorrection/my?employeeId=5
        // =====================================================
        [HttpGet("my")]
        public async Task<IActionResult> GetMyCorrections([FromQuery] long employeeId)
        {
            try
            {
                var list = await _correctionService.GetEmployeeCorrectionsAsync(employeeId);
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
        // GET PENDING CORRECTIONS (MANAGER / ADMIN)
        // GET: api/AttendanceCorrection/pending?branchId=1
        // =====================================================
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingCorrections([FromQuery] int? branchId)
        {
            try
            {
                var list = await _correctionService.GetPendingCorrectionsAsync(branchId);
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
        // REVIEW CORRECTION (APPROVE / REJECT)
        // PUT: api/AttendanceCorrection/{id}/review?reviewerId=1
        // =====================================================
        [HttpPut("{id:long}/review")]
        public async Task<IActionResult> ReviewCorrection(
            long id,
            [FromQuery] long reviewerId,
            [FromBody] ReviewAttendanceCorrectionDto review)
        {
            try
            {
                var result = await _correctionService.ReviewCorrectionAsync(id, reviewerId, review);
                var action = review.Status == 1 ? "approved" : "rejected";
                return Ok(new
                {
                    success = true,
                    message = $"Attendance correction request {action} successfully.",
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
        // CANCEL CORRECTION
        // PUT: api/AttendanceCorrection/{id}/cancel?employeeId=5
        // =====================================================
        [HttpPut("{id:long}/cancel")]
        public async Task<IActionResult> CancelCorrection(
            long id,
            [FromQuery] long employeeId)
        {
            try
            {
                await _correctionService.CancelCorrectionAsync(id, employeeId);
                return Ok(new
                {
                    success = true,
                    message = "Attendance correction request cancelled successfully."
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
