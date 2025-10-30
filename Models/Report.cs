using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BlogWebsite.Models
{
    public class Report
    {
        public int ReportId { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Sử dụng kiểu Enum để đảm bảo dữ liệu
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        // Khóa ngoại đến Post (Bị báo cáo)
        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        // Khóa ngoại đến AppUser (Người báo cáo)
        public string AppUserId { get; set; }
        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; }
    }
}
