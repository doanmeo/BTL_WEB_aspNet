using System.ComponentModel.DataAnnotations;

namespace BlogWebsite.Models.ViewModels
{
    public class ReportViewModel
    {
        [Required]
        public int PostId { get; set; }
        public string? PostAuthorName { get; set; }      // Tên người bị report
        public string? PostContentSnippet { get; set; }
        [Required(ErrorMessage = "Reason for report is required.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Reason must be between 5 and 500 characters.")]
        public string Reason { get; set; }
    }
}
