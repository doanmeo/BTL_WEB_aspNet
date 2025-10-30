using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Composition;
namespace BlogWebsite.Models
{
    public class Post
    {
        public int PostId { get; set; }

        [Required]
        public string Content { get; set; } // Sẽ là NVARCHAR(MAX)

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Khóa ngoại đến Thread
        public int ThreadId { get; set; }
        [ForeignKey("ThreadId")]
        public virtual Thread Thread { get; set; }

        // Khóa ngoại đến AppUser (Người tạo)
        public string AppUserId { get; set; }
        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; }

        // Liên kết 1-Nhiều
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
    }
}
