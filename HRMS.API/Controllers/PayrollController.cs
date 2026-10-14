using System;
using System.Threading.Tasks;
using HRMS.API.DTOs.Payroll;
using HRMS.API.Service.Payroll;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;

        public PayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        // =====================================================
        // GET PAYROLL PERIODS
        // GET: api/Payroll/periods?companyId=1&year=2026
        // =====================================================
        [HttpGet("periods")]
        public async Task<IActionResult> GetPeriods([FromQuery] long companyId = 1, [FromQuery] int? year = null)
        {
            try
            {
                var result = await _payrollService.GetPeriodsAsync(companyId, year);
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
        // PROCESS PAYROLL RUN (BATCH BATCH ENGINE)
        // POST: api/Payroll/process?processedBy=1
        // =====================================================
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayroll([FromBody] ProcessPayrollRequestDto request, [FromQuery] long processedBy = 1)
        {
            try
            {
                var result = await _payrollService.ProcessPayrollAsync(request, processedBy);
                return Ok(new
                {
                    success = true,
                    message = "Payroll processed successfully as Draft.",
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
        // GET PAYROLL RUNS
        // GET: api/Payroll/runs?companyId=1&branchId=2
        // =====================================================
        [HttpGet("runs")]
        public async Task<IActionResult> GetRuns([FromQuery] long companyId = 1, [FromQuery] int? branchId = null)
        {
            try
            {
                var result = await _payrollService.GetRunsAsync(companyId, branchId);
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
        // GET PAYROLL RUN DETAILS
        // GET: api/Payroll/run/5
        // =====================================================
        [HttpGet("run/{id:long}")]
        public async Task<IActionResult> GetRunDetails(long id)
        {
            try
            {
                var result = await _payrollService.GetRunDetailsAsync(id);
                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Payroll run not found."
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
        // UPDATE PAYROLL RUN STATUS (WORKFLOW TRANSITIONS)
        // PUT: api/Payroll/run/5/status?reviewerId=1
        // =====================================================
        [HttpPut("run/{id:long}/status")]
        public async Task<IActionResult> UpdateRunStatus(
            long id,
            [FromBody] UpdatePayrollStatusDto dto,
            [FromQuery] long reviewerId = 1)
        {
            try
            {
                var result = await _payrollService.UpdateRunStatusAsync(id, dto.Status, reviewerId, dto.Comments);
                return Ok(new
                {
                    success = true,
                    message = $"Payroll status transitioned to {result.StatusName}.",
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
        // GET EMPLOYEE PAYSLIP LIST (SELF-SERVICE)
        // GET: api/Payroll/employee/5/payslips
        // =====================================================
        [HttpGet("employee/{employeeId:long}/payslips")]
        public async Task<IActionResult> GetEmployeePayslips(long employeeId)
        {
            try
            {
                var result = await _payrollService.GetEmployeePayslipsAsync(employeeId);
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
        // GET PAYSLIP DETAILS WITH ITEMIZED BREAKDOWN
        // GET: api/Payroll/payslip/10
        // =====================================================
        [HttpGet("payslip/{id:long}")]
        public async Task<IActionResult> GetPayslipDetails(long id)
        {
            try
            {
                var result = await _payrollService.GetPayslipDetailsAsync(id);
                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Payslip not found."
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
