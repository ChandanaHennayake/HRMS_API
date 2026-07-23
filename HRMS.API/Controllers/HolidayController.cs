using HRMS.API.DTOs.Holiday;
using HRMS.API.Service.Holiday;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayService _holidayService;

        public HolidayController(
            IHolidayService holidayService)
        {
            _holidayService = holidayService;
        }


        // =====================================================
        // CREATE HOLIDAY
        // POST: api/Holiday
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateHolidayDto request)
        {
            try
            {
                var result =
                    await _holidayService.CreateAsync(request);

                return Ok(new
                {
                    success = true,
                    message = "Holiday created successfully.",
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
        // GET ALL HOLIDAYS
        // GET: api/Holiday
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result =
                    await _holidayService.GetAllAsync();

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
        // GET HOLIDAY BY ID
        // GET: api/Holiday/5
        // =====================================================

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(
            long id)
        {
            try
            {
                var result =
                    await _holidayService.GetByIdAsync(id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Holiday not found."
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


        // =====================================================
        // UPDATE HOLIDAY
        // PUT: api/Holiday/5
        // =====================================================

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(
            long id,
            [FromBody] UpdateHolidayDto request)
        {
            try
            {
                var result =
                    await _holidayService.UpdateAsync(
                        id,
                        request);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Holiday not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Holiday updated successfully.",
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
        // DELETE / DEACTIVATE HOLIDAY
        // DELETE: api/Holiday/5
        // =====================================================

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(
            long id,
            [FromQuery] long? deletedBy)
        {
            try
            {
                var result =
                    await _holidayService.DeleteAsync(
                        id,
                        deletedBy);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Holiday not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Holiday deleted successfully."
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
        // CHECK WHETHER DATE IS HOLIDAY
        //
        // GET:
        // api/Holiday/check?companyId=5&date=2026-12-25
        // =====================================================

        [HttpGet("check")]
        public async Task<IActionResult> IsHoliday(
            [FromQuery] long companyId,
            [FromQuery] DateOnly date)
        {
            try
            {
                var isHoliday =
                    await _holidayService.IsHolidayAsync(
                        companyId,
                        date);

                return Ok(new
                {
                    success = true,
                    companyId,
                    date,
                    isHoliday
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