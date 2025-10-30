using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Composition;
namespace BlogWebsite.Models
{
    public class AppUser : IdentityUser
    {
        // Liên kết 1-1 với UserProfile
        public virtual UserProfile UserProfile { get; set; }

        // Liên kết 1-Nhiều: Một User có thể tạo nhiều...
        public virtual ICollection<Forum> Forums { get; set; }
        public virtual ICollection<Thread> Threads { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
    }
}
