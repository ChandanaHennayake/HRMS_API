namespace HRMS.API.DTOs.Holiday
{
    public class UpdateHolidayDto
    {
        public DateOnly HolidayDate { get; set; }

        public string HolidayName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
