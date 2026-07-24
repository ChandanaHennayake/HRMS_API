using ABANS_BAN.DTOs.Attendance;
using HRMS.API.Service.Attendance;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(
            IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // =====================================================
        // CHECK IN
        // POST: api/Attendance/check-in?employeeId=5
        // =====================================================

        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn(
            [FromQuery] long employeeId,
            [FromBody] CheckInRequestDto request)
        {
            try
            {
                var result = await _attendanceService
                    .CheckInAsync(employeeId, request);

                return Ok(new
                {
                    success = true,
                    message = "Check-in successful.",
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
        // CHECK OUT
        // POST: api/Attendance/check-out?employeeId=5
        // =====================================================

        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut(
            [FromQuery] long employeeId,
            [FromBody] CheckOutRequestDto request)
        {
            try
            {
                var result = await _attendanceService
                    .CheckOutAsync(employeeId, request);

                return Ok(new
                {
                    success = true,
                    message = "Check-out successful.",
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
        // GET TODAY ATTENDANCE
        // GET: api/Attendance/today?employeeId=5
        // =====================================================

        [HttpGet("today")]
        public async Task<IActionResult> GetTodayAttendance(
            [FromQuery] long employeeId)
        {
            try
            {
                var result = await _attendanceService
                    .GetTodayAttendanceAsync(employeeId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "No attendance record found for today."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message =
                        "Attendance retrieved successfully.",
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
    }
}