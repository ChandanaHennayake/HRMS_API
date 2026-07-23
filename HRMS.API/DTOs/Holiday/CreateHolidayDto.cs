namespace HRMS.API.DTOs.Holiday
{
    public class CreateHolidayDto
    {
        public long CompanyId { get; set; }

        public DateOnly HolidayDate { get; set; }

        public string HolidayName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public long? CreatedBy { get; set; }
    }
}
