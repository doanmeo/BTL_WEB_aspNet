using System.ComponentModel.DataAnnotations;

namespace BlogWebsite.Models.ViewModels
{
    public class PostViewModel
    {
        [Required]
        public int ThreadId { get; set; }

        [Required(ErrorMessage = "Reply content cannot be empty.")]
        [StringLength(40000, MinimumLength = 5, ErrorMessage = "The reply must be between 5 and 40000 characters.")]
        public string Content { get; set; }
    }
}
