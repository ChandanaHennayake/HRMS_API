namespace HRMS.API.DTOs.Branch
{
    public class BranchDto
    {
        public int BranchId { get; set; }

        public int CompanyId { get; set; }

        public string? BranchCode { get; set; }

        public string BranchName { get; set; } = null!;

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public int GeofenceRadiusMeters { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}