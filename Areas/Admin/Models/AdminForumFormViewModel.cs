using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlogWebsite.Areas.Admin.Models
{
    public class AdminForumFormViewModel
    {
        public int? ForumId { get; set; }

        [Required]
        [Display(Name = "Tên diễn đàn")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Chuyên mục")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn chuyên mục")]
        public int? CategoryId { get; set; }

        [Required]
        [Display(Name = "Quản trị viên phụ trách")]
        public string? AppUserId { get; set; }

        [Display(Name = "Ẩn diễn đàn")]
        public bool IsDeleted { get; set; }

        public IEnumerable<SelectListItem> CategoryOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> UserOptions { get; set; } = new List<SelectListItem>();
    }
}

