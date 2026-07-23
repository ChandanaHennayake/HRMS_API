namespace HRMS.API.DTOs.Holiday
{
    public class HolidayDto
    {
        public long Id { get; set; }

        public long CompanyId { get; set; }

        public DateOnly HolidayDate { get; set; }

        public string HolidayName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
