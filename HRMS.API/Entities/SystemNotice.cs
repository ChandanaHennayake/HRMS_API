using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Entities
{
    [Table("SystemNotice")]
    public class SystemNotice
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Message { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string TargetRole { get; set; } = "All";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }
    }
}
