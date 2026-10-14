using System;
using System.Threading.Tasks;
using HRMS.API.DTOs.Report;
using HRMS.API.Service.Report;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // =====================================================
        // ATTENDANCE REPORT
        // GET: api/Report/attendance?branchId=1&startDate=2026-10-01&endDate=2026-10-14
        // =====================================================
        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendanceReport([FromQuery] AttendanceReportFilterDto filter)
        {
            try
            {
                var result = await _reportService.GetAttendanceReportAsync(filter);
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
        // EXPORT ATTENDANCE CSV
        // GET: api/Report/attendance/export-csv
        // =====================================================
        [HttpGet("attendance/export-csv")]
        public async Task<IActionResult> ExportAttendanceCsv([FromQuery] AttendanceReportFilterDto filter)
        {
            try
            {
                var bytes = await _reportService.GenerateAttendanceCsvAsync(filter);
                var fileName = $"attendance_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                return File(bytes, "text/csv", fileName);
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
        // LEAVE REPORT
        // GET: api/Report/leave?branchId=1&startDate=2026-01-01
        // =====================================================
        [HttpGet("leave")]
        public async Task<IActionResult> GetLeaveReport([FromQuery] LeaveReportFilterDto filter)
        {
            try
            {
                var result = await _reportService.GetLeaveReportAsync(filter);
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
        // EXPORT LEAVE CSV
        // GET: api/Report/leave/export-csv
        // =====================================================
        [HttpGet("leave/export-csv")]
        public async Task<IActionResult> ExportLeaveCsv([FromQuery] LeaveReportFilterDto filter)
        {
            try
            {
                var bytes = await _reportService.GenerateLeaveCsvAsync(filter);
                var fileName = $"leave_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                return File(bytes, "text/csv", fileName);
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
        // PAYROLL REPORT
        // GET: api/Report/payroll?periodId=1&branchId=1
        // =====================================================
        [HttpGet("payroll")]
        public async Task<IActionResult> GetPayrollReport([FromQuery] PayrollReportFilterDto filter)
        {
            try
            {
                var result = await _reportService.GetPayrollReportAsync(filter);
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
        // EXPORT PAYROLL CSV
        // GET: api/Report/payroll/export-csv
        // =====================================================
        [HttpGet("payroll/export-csv")]
        public async Task<IActionResult> ExportPayrollCsv([FromQuery] PayrollReportFilterDto filter)
        {
            try
            {
                var bytes = await _reportService.GeneratePayrollCsvAsync(filter);
                var fileName = $"payroll_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                return File(bytes, "text/csv", fileName);
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
        // HEADCOUNT & WORKFORCE DISTRIBUTION
        // GET: api/Report/headcount?branchId=1
        // =====================================================
        [HttpGet("headcount")]
        public async Task<IActionResult> GetHeadcountReport([FromQuery] int? branchId = null)
        {
            try
            {
                var result = await _reportService.GetHeadcountReportAsync(branchId);
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
