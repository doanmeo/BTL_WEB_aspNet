using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogWebsite.Models
{
    // Bảng trung gian để theo dõi User nào đang "Watch" Thread nào
    public class WatchedThread
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string AppUserId { get; set; }

        [Required]
        public int ThreadId { get; set; }

        public DateTime WatchedAt { get; set; }

        // Navigation properties
        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; }

        [ForeignKey("ThreadId")]
        public virtual Thread Thread { get; set; }
    }
}
