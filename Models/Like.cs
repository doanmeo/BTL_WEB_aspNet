using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace BlogWebsite.Models
{
    public class Like
    {
        public int LikeId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Khóa ngoại đến Post
        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        // Khóa ngoại đến AppUser (Người like)
        public string AppUserId { get; set; }
        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; }
    }
}
