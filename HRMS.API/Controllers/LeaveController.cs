using HRMS.API.DTOs.Leave;
using HRMS.API.Service.Leave;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;

        public LeaveController(
            ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }


        // =====================================================
        // APPLY LEAVE
        //
        // POST:
        // api/Leave/apply?employeeId=5
        // =====================================================

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLeave(
            [FromQuery] long employeeId,
            [FromBody] ApplyLeaveDto request)
        {
            try
            {
                var result = await _leaveService
                    .ApplyLeaveAsync(
                        employeeId,
                        request);

                return Ok(new
                {
                    success = true,
                    message =
                        "Leave request submitted successfully.",
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
        // GET EMPLOYEE LEAVE HISTORY
        //
        // GET:
        // api/Leave/employee/5
        // =====================================================

        [HttpGet("employee/{employeeId:long}")]
        public async Task<IActionResult> GetEmployeeLeaves(
            long employeeId)
        {
            try
            {
                var result = await _leaveService
                    .GetEmployeeLeavesAsync(
                        employeeId);

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
        // GET EMPLOYEE LEAVE BALANCES
        //
        // GET:
        // api/Leave/employee/5/balance?year=2026
        // =====================================================

        [HttpGet("employee/{employeeId:long}/balance")]
        public async Task<IActionResult> GetEmployeeLeaveBalances(
            long employeeId,
            [FromQuery] int? year)
        {
            try
            {
                var result = await _leaveService
                    .GetEmployeeLeaveBalancesAsync(employeeId, year);

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


        // GET:
        // api/Leave/balance?employeeId=5&year=2026
        [HttpGet("balance")]
        public async Task<IActionResult> GetLeaveBalancesByQuery(
            [FromQuery] long employeeId,
            [FromQuery] int? year)
        {
            try
            {
                var result = await _leaveService
                    .GetEmployeeLeaveBalancesAsync(employeeId, year);

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
        // GET ALL PENDING LEAVE REQUESTS
        //
        // GET:
        // api/Leave/pending
        // =====================================================

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingLeaves([FromQuery] int? branchId = null)
        {
            try
            {
                var result = await _leaveService
                    .GetPendingLeavesAsync(branchId);

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
        // APPROVE LEAVE
        //
        // PUT:
        // api/Leave/10/approve?approvedBy=5
        // =====================================================

        [HttpPut("{leaveRequestId:long}/approve")]
        public async Task<IActionResult> ApproveLeave(
            long leaveRequestId,
            [FromQuery] long approvedBy,
            [FromBody] LeaveActionDto request)
        {
            try
            {
                var result = await _leaveService
                    .ApproveLeaveAsync(
                        leaveRequestId,
                        approvedBy,
                        request);

                return Ok(new
                {
                    success = true,
                    message =
                        "Leave request approved successfully.",
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
        // REJECT LEAVE
        //
        // PUT:
        // api/Leave/10/reject?rejectedBy=5
        // =====================================================

        [HttpPut("{leaveRequestId:long}/reject")]
        public async Task<IActionResult> RejectLeave(
            long leaveRequestId,
            [FromQuery] long rejectedBy,
            [FromBody] LeaveActionDto request)
        {
            try
            {
                var result = await _leaveService
                    .RejectLeaveAsync(
                        leaveRequestId,
                        rejectedBy,
                        request);

                return Ok(new
                {
                    success = true,
                    message =
                        "Leave request rejected successfully.",
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
        // CANCEL LEAVE
        //
        // PUT:
        // api/Leave/10/cancel?employeeId=5
        // =====================================================

        [HttpPut("{leaveRequestId:long}/cancel")]
        public async Task<IActionResult> CancelLeave(
            long leaveRequestId,
            [FromQuery] long employeeId)
        {
            try
            {
                var result = await _leaveService
                    .CancelLeaveAsync(
                        leaveRequestId,
                        employeeId);

                return Ok(new
                {
                    success = true,
                    message =
                        "Leave request cancelled successfully.",
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