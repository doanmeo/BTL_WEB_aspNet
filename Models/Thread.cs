using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogWebsite.Models
{
    public class Thread
    {
        public int ThreadId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public int ViewCount { get; set; } = 0;
        public bool IsLocked { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        // Khóa ngoại đến Forum
        public int ForumId { get; set; }
        [ForeignKey("ForumId")]
        public virtual Forum Forum { get; set; }

        // Khóa ngoại đến AppUser (Người tạo)
        public string AppUserId { get; set; }
        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; }

        // Liên kết 1-Nhiều: Một Thread có nhiều Post
        public virtual ICollection<Post> Posts { get; set; }
    }
}
