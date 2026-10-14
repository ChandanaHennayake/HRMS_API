namespace HRMS.API.DTOs.Lookup
{
    /// <summary>
    /// Generic master lookup item used to populate dropdowns.
    /// </summary>
    public class LookupDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Branch lookup item including geofence details.
    /// </summary>
    public class BranchLookupDto : LookupDto
    {
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int GeofenceRadiusMeters { get; set; }
    }
}
