using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BlogWebsite.Models
{
        public class Forum
        {
            public int ForumId { get; set; }

            [Required]
            [StringLength(100)]
            public string Name { get; set; }

            [StringLength(255)]
            public string Description { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public DateTime? UpdatedAt { get; set; } // Dấu ? cho phép NULL
            public bool IsDeleted { get; set; } = false;

            // Khóa ngoại đến Category
            public int CategoryId { get; set; }
            [ForeignKey("CategoryId")]
            public virtual Category Category { get; set; }

            // Khóa ngoại đến AppUser (Người tạo)
            public string AppUserId { get; set; }
            [ForeignKey("AppUserId")]
            public virtual AppUser AppUser { get; set; }

            // Liên kết 1-Nhiều: Một Forum có nhiều Thread
            public virtual ICollection<Thread> Threads { get; set; }
        }
    }

