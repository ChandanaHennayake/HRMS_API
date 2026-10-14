using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Entities
{
    [Table("Notification")]
    public class Notification
    {
        [Key]
        public long Id { get; set; }

        public int UserId { get; set; }

        public long? EmployeeId { get; set; }

        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string Type { get; set; } = "Info";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; }

        [StringLength(50)]
        public string? ReferenceType { get; set; }

        public long? ReferenceId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }
    }
}
