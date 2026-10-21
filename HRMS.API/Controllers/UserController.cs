using HRMS.API.DTOs.User;
using HRMS.API.Service.User;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Create User
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserRequest request)
        {
            var userId = await _userService.CreateAsync(request);

            return Ok(new
            {
                Success = true,
                Message = "User created successfully.",
                Data = userId
            });
        }

        /// <summary>
        /// Get All Users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();

            return Ok(new
            {
                Success = true,
                Data = users
            });
        }

        /// <summary>
        /// Get User By Id
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "User not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Data = user
            });
        }

        /// <summary>
        /// Get Complete User Profile by Id
        /// </summary>
        [HttpGet("profile/{id:int}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var profile = await _userService.GetProfileAsync(id);

            if (profile == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "User profile not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Data = profile
            });
        }

        /// <summary>
        /// Change User Password
        /// </summary>
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                await _userService.ChangePasswordAsync(request);

                return Ok(new
                {
                    Success = true,
                    Message = "Password changed successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Admin Change / Reset User Password
        /// </summary>
        [HttpPost("admin/reset-password")]
        public async Task<IActionResult> AdminResetPassword([FromBody] AdminResetPasswordRequest request)
        {
            try
            {
                await _userService.AdminResetPasswordAsync(request);

                return Ok(new
                {
                    Success = true,
                    Message = "User password successfully changed by administrator."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Update User
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateUserRequest request)
        {
            await _userService.UpdateAsync(id, request);

            return Ok(new
            {
                Success = true,
                Message = "User updated successfully."
            });
        }

        /// <summary>
        /// Delete User (Soft Delete)
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteAsync(id);

            return Ok(new
            {
                Success = true,
                Message = "User deleted successfully."
            });
        }
    }
}