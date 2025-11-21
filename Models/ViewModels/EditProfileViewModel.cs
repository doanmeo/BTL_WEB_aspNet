using System.ComponentModel.DataAnnotations;

namespace BlogWebsite.Models.ViewModels
{
    public class EditProfileViewModel
    {
        // Không cần UserId ở đây vì nó sẽ được lấy từ người dùng đăng nhập

        [StringLength(100, ErrorMessage = "Display Name cannot exceed 100 characters.")]
        public string? DisplayName { get; set; }

        [DataType(DataType.Url)]
        [StringLength(255, ErrorMessage = "Avatar URL cannot exceed 255 characters.")]
        public string? AvatarUrl { get; set; }

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        public string? Bio { get; set; }
    }
}
