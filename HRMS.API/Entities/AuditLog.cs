using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Entities
{
    [Table("AuditLog")]
    public class AuditLog
    {
        [Key]
        public long Id { get; set; }

        public int? UserId { get; set; }

        [StringLength(100)]
        public string? UserName { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EntityId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "text")]
        public string? OldValues { get; set; }

        [Column(TypeName = "text")]
        public string? NewValues { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(250)]
        public string? UserAgent { get; set; }

        [StringLength(30)]
        public string Severity { get; set; } = "Info";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }
    }
}
