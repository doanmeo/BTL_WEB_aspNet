using System.ComponentModel.DataAnnotations;

namespace BlogWebsite.Areas.Admin.Models
{
    public class AdminUserEditViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Đã xác thực email")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Tên hiển thị")]
        public string? DisplayName { get; set; }

        [Display(Name = "Ảnh đại diện (URL)")]
        public string? AvatarUrl { get; set; }

        [Display(Name = "Giới thiệu")]
        public string? Bio { get; set; }
    }
}

