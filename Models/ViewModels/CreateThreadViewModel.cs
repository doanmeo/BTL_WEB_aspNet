using System.ComponentModel.DataAnnotations;

namespace BlogWebsite.Models.ViewModels
{
    public class CreateThreadViewModel
    {
        [Required]
        public int ForumId { get; set; }

        public string ForumName { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 150 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content for the first post is required.")]
        [StringLength(20000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 20,000 characters.")]
        public string Content { get; set; }
        
    }
}
