namespace ABANS_BAN.DTOs.Attendance
{
    public class CheckOutRequestDto
    {
        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        /// <summary>
        /// Optional client device timestamp. Validated against server time.
        /// </summary>
        public DateTime? DeviceTime { get; set; }
    }
}