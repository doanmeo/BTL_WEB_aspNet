using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BlogWebsite.Models
{
    public class UserProfile
    {
        [Key] // Đặt UserId làm khóa chính
        [ForeignKey("AppUser")] // Liên kết 1-1 với AppUser
        public string UserId { get; set; }

        [StringLength(100)]
        public string DisplayName { get; set; }

        [StringLength(255)]
        public string AvatarUrl { get; set; }

        [StringLength(500)]
        public string Bio { get; set; }

        public int Reputation { get; set; }
        public DateTime JoinedAt { get; set; }

        // Khai báo mối quan hệ
        public virtual AppUser User { get; set; }
    }
}
