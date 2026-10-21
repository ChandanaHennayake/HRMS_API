namespace HRMS.API.DTOs.User
{
    public class AdminResetPasswordRequest
    {
        public int UserId { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}
