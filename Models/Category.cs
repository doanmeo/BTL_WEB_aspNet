using BlogWebsite.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogWebsite.Models // <-- Đảm bảo namespace này khớp với dự án của bạn
{
    public class Category
    {
        [Key] // Báo cho EF Core đây là khóa chính
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên chuyên mục là bắt buộc")]
        [StringLength(100)]
        public string Name { get; set; } // Lỗi của bạn là do thiếu thuộc tính này

        [StringLength(255)]
        public string Description { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Khai báo mối quan hệ: Một Category có nhiều Forum
        public virtual ICollection<Forum> Forums { get; set; }
    }
}