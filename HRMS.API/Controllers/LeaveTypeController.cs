using HRMS.API.Entities;
using HRMS.API.Service.Leave;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveTypeController : ControllerBase
    {
        private readonly ILeaveTypeService _leaveTypeService;

        public LeaveTypeController(
            ILeaveTypeService leaveTypeService)
        {
            _leaveTypeService = leaveTypeService;
        }

        // =====================================================
        // GET ALL LEAVE TYPES
        //
        // GET:
        // api/LeaveType
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result =
                    await _leaveTypeService.GetAllAsync();

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
        // GET ACTIVE LEAVE TYPES
        //
        // GET:
        // api/LeaveType/active
        // =====================================================

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var result =
                    await _leaveTypeService.GetActiveAsync();

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
        // GET BY ID
        //
        // GET:
        // api/LeaveType/1
        // =====================================================

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var result =
                    await _leaveTypeService.GetByIdAsync(id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Leave type not found."
                    });
                }

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
    }
}