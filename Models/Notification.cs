using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogWebsite.Models
{
    public enum NotificationType
    {
        Like = 0,
        Reply = 1,
        System = 2
    }

    public class Notification
    {
        public int NotificationId { get; set; }

        // Người nhận thông báo
        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual AppUser User { get; set; }

        // Người tạo hành động (like, reply), có thể null cho thông báo hệ thống
        public string? FromUserId { get; set; }

        [ForeignKey(nameof(FromUserId))]
        public virtual AppUser? FromUser { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [StringLength(255)]
        public string Message { get; set; }

        [StringLength(255)]
        public string? Link { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}


