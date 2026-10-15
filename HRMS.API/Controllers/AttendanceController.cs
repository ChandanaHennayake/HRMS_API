using ABANS_BAN.DTOs.Attendance;
using HRMS.API.Repository.UserDetails;
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
        private readonly IUserRepository _userRepository;

        public AttendanceController(
            IAttendanceService attendanceService,
            IUserRepository userRepository)
        {
            _attendanceService = attendanceService;
            _userRepository = userRepository;
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
                    ?? User.FindFirst("employeeId")?.Value;

                long loggedInEmployeeId = 0;
                if (!string.IsNullOrEmpty(employeeIdClaim))
                {
                    long.TryParse(employeeIdClaim, out loggedInEmployeeId);
                }

                // If loggedInEmployeeId is not found from claim, lookup from authenticated user's id
                if (loggedInEmployeeId <= 0)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                   ?? User.FindFirst("sub")?.Value
                                   ?? User.FindFirst("UserId")?.Value;

                    if (int.TryParse(userIdClaim, out var uid))
                    {
                        var user = await _userRepository.GetByIdAsync(uid);
                        if (user?.EmployeeId != null && user.EmployeeId.Value > 0)
                        {
                            loggedInEmployeeId = user.EmployeeId.Value;
                        }
                    }
                }

                // Fallback: If query parameter employeeId is provided
                if (loggedInEmployeeId <= 0 && employeeId.HasValue && employeeId.Value > 0)
                {
                    loggedInEmployeeId = employeeId.Value;
                }

                var isAdmin =
                    User.IsInRole("Admin") ||
                    User.IsInRole("SuperAdmin") ||
                    User.IsInRole("Manager");

                if (loggedInEmployeeId <= 0 && !isAdmin)
                {
                    return Unauthorized(
                        new
                        {
                            message = "Employee ID not found in token."
                        });
                }

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

        // =====================================================
        // GET BRANCH / COMPANY DAILY ATTENDANCE ROSTER
        // GET: api/Attendance/branch-daily?branchId=1&date=2026-10-13
        // GET: api/Attendance/company-today?branchId=1&date=2026-10-13
        // =====================================================

        [HttpGet("branch-daily")]
        [HttpGet("company-today")]
        public async Task<IActionResult> GetBranchDailyAttendance(
            [FromQuery] int? branchId,
            [FromQuery] DateOnly? date)
        {
            try
            {
                var roster = await _attendanceService.GetBranchDailyAttendanceAsync(branchId, date);
                return Ok(new
                {
                    success = true,
                    data = roster
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