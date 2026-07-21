using HRMS.API.DTOs.Branch;
using HRMS.API.Service.Branch;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _branchService;

        public BranchController(
            IBranchService branchService)
        {
            _branchService = branchService;
        }


        // =====================================================
        // CREATE
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateBranchDto request)
        {
            try
            {
                var result =
                    await _branchService
                        .CreateAsync(request);


                return Ok(new
                {
                    success = true,

                    message =
                        "Branch created successfully.",

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
        // GET ALL
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result =
                await _branchService
                    .GetAllAsync();


            return Ok(new
            {
                success = true,

                data = result
            });
        }


        // =====================================================
        // GET BY ID
        // =====================================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(
            int id)
        {
            var result =
                await _branchService
                    .GetByIdAsync(id);


            if (result == null)
            {
                return NotFound(new
                {
                    success = false,

                    message =
                        "Branch not found."
                });
            }


            return Ok(new
            {
                success = true,

                data = result
            });
        }


        // =====================================================
        // UPDATE
        // =====================================================

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateBranchDto request)
        {
            try
            {
                var result =
                    await _branchService
                        .UpdateAsync(
                            id,
                            request);


                return Ok(new
                {
                    success = true,

                    message =
                        "Branch updated successfully.",

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
        // DELETE / DEACTIVATE
        // =====================================================

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                await _branchService
                    .DeleteAsync(id);


                return Ok(new
                {
                    success = true,

                    message =
                        "Branch deactivated successfully."
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