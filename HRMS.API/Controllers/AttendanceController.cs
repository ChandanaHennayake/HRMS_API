using ABANS_BAN.DTOs.Attendance;
using HRMS.API.Service.Attendance;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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


        [HttpGet("history")]
        public async Task<IActionResult> GetAttendanceHistory(
           [FromQuery] long? employeeId,
           [FromQuery] DateOnly? fromDate,
           [FromQuery] DateOnly? toDate)
        {
            try
            {
                // ---------------------------------------------
                // GET LOGGED-IN EMPLOYEE ID
                // ---------------------------------------------

                var employeeIdClaim =
                    User.FindFirst("EmployeeId")?.Value
                    ??
                    User.FindFirst(
                        ClaimTypes.NameIdentifier)?.Value;

                if (!long.TryParse(
                        employeeIdClaim,
                        out var loggedInEmployeeId))
                {
                    return Unauthorized(
                        new
                        {
                            message =
                                "Employee ID not found in token."
                        });
                }


                // ---------------------------------------------
                // CHECK ADMIN
                // ---------------------------------------------

                var isAdmin =
                    User.IsInRole("Admin") ||
                    User.IsInRole("SuperAdmin");


                // ---------------------------------------------
                // GET HISTORY
                // ---------------------------------------------

                var result =
                    await _attendanceService
                        .GetAttendanceHistoryAsync(
                            loggedInEmployeeId,
                            isAdmin,
                            employeeId,
                            fromDate,
                            toDate);


                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new
                    {
                        message = ex.Message
                    });
            }
        }

    }
}