using HRMS.API.DTOs.Employee;
using HRMS.API.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // =========================================================
        // CREATE EMPLOYEE
        // POST: api/Employee
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateEmployeeDto request)
        {
            try
            {
                var result = await _employeeService.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    result
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        // =========================================================
        // GET ALL EMPLOYEES
        // GET: api/Employee
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _employeeService.GetAllAsync();

            return Ok(result);
        }


        // =========================================================
        // GET EMPLOYEE BY ID
        // GET: api/Employee/1
        // =========================================================
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _employeeService.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Employee not found."
                });
            }

            return Ok(result);
        }


        // =========================================================
        // UPDATE EMPLOYEE
        // PUT: api/Employee/1
        // =========================================================
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(
            long id,
            [FromBody] UpdateEmployeeDto request)
        {
            try
            {
                var result = await _employeeService.UpdateAsync(
                    id,
                    request
                );

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Employee not found."
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        // =========================================================
        // SOFT DELETE EMPLOYEE
        // DELETE: api/Employee/1?deletedBy=1
        // =========================================================
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(
            long id,
            [FromQuery] long? deletedBy)
        {
            var result = await _employeeService.DeleteAsync(
                id,
                deletedBy
            );

            if (!result)
            {
                return NotFound(new
                {
                    message = "Employee not found."
                });
            }

            return Ok(new
            {
                message = "Employee deleted successfully."
            });
        }
    }
}