using System;
using System.Threading.Tasks;
using HRMS.API.DTOs.Salary;
using HRMS.API.Service.Salary;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalaryController : ControllerBase
    {
        private readonly ISalaryService _salaryService;

        public SalaryController(ISalaryService salaryService)
        {
            _salaryService = salaryService;
        }

        // =====================================================
        // CALCULATE SALARY (TEST FIXTURE & DETERMINISTIC ENGINE)
        // POST: api/Salary/calculate
        // =====================================================
        [HttpPost("calculate")]
        public IActionResult CalculateSalary([FromBody] SalaryCalculationRequestDto request)
        {
            try
            {
                var result = _salaryService.CalculateSalary(request);
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
        // GET EMPLOYEE SALARY PROFILE
        // GET: api/Salary/employee/5
        // =====================================================
        [HttpGet("employee/{employeeId:long}")]
        public async Task<IActionResult> GetEmployeeSalary(long employeeId)
        {
            try
            {
                var result = await _salaryService.GetEmployeeSalaryAsync(employeeId);
                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Employee not found."
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
        // SAVE EMPLOYEE SALARY
        // POST: api/Salary/employee
        // =====================================================
        [HttpPost("employee")]
        public async Task<IActionResult> SaveEmployeeSalary([FromBody] SaveEmployeeSalaryDto request)
        {
            try
            {
                var result = await _salaryService.SaveEmployeeSalaryAsync(request);
                return Ok(new
                {
                    success = true,
                    message = "Employee salary profile saved successfully.",
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
        // GET SALARY COMPONENTS
        // GET: api/Salary/components?companyId=1
        // =====================================================
        [HttpGet("components")]
        public async Task<IActionResult> GetComponents([FromQuery] long companyId = 1)
        {
            try
            {
                var result = await _salaryService.GetComponentsAsync(companyId);
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
        // CREATE SALARY COMPONENT
        // POST: api/Salary/component
        // =====================================================
        [HttpPost("component")]
        public async Task<IActionResult> CreateComponent([FromBody] SalaryComponentDto request)
        {
            try
            {
                var result = await _salaryService.CreateComponentAsync(request);
                return Ok(new
                {
                    success = true,
                    message = "Salary component created successfully.",
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
        // GET SALARY STRUCTURES
        // GET: api/Salary/structures?companyId=1
        // =====================================================
        [HttpGet("structures")]
        public async Task<IActionResult> GetStructures([FromQuery] long companyId = 1)
        {
            try
            {
                var result = await _salaryService.GetStructuresAsync(companyId);
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
