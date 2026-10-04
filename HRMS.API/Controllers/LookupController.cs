using HRMS.API.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    /// <summary>
    /// Master lookup endpoints used to populate dropdowns
    /// (employee form, filters, etc.).
    /// Optional companyId scopes company-owned masters.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LookupController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public LookupController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        // GET: api/Lookup/departments?companyId=1
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments([FromQuery] long? companyId)
            => Ok(await _lookupService.GetDepartmentsAsync(companyId));

        // GET: api/Lookup/designations?companyId=1
        [HttpGet("designations")]
        public async Task<IActionResult> GetDesignations([FromQuery] long? companyId)
            => Ok(await _lookupService.GetDesignationsAsync(companyId));

        // GET: api/Lookup/employment-types
        [HttpGet("employment-types")]
        public async Task<IActionResult> GetEmploymentTypes()
            => Ok(await _lookupService.GetEmploymentTypesAsync());

        // GET: api/Lookup/roles?companyId=1
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles([FromQuery] long? companyId)
            => Ok(await _lookupService.GetRolesAsync(companyId));

        // GET: api/Lookup/branches?companyId=1
        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches([FromQuery] long? companyId)
            => Ok(await _lookupService.GetBranchesAsync(companyId));
    }
}
