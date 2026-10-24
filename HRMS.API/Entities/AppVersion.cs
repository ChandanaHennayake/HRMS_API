using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Entities
{
    [Table("AppVersion")]
    public class AppVersion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Platform { get; set; } = "Android"; // "Android", "iOS", "Web"

        [Required]
        [StringLength(50)]
        public string LatestVersion { get; set; } = "1.0.3";

        [Required]
        [StringLength(50)]
        public string MinimumVersion { get; set; } = "1.0.3";

        public bool ForceUpdate { get; set; } = true;

        [StringLength(500)]
        public string? UpdateUrl { get; set; }

        [StringLength(500)]
        public string? ReleaseNotes { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
