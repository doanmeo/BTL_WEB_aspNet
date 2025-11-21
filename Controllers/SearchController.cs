using BlogWebsite.Data;
using BlogWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogWebsite.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Search/Results?q=tukhoa
        [HttpGet]
        public async Task<IActionResult> Results(string q)
        {
            // Nếu từ khóa rỗng, trả về danh sách rỗng hoặc quay lại trang chủ
            if (string.IsNullOrWhiteSpace(q))
            {
                return View(new List<BlogWebsite.Models.Thread>());
            }

            // Tìm kiếm trong bảng Thread theo Tiêu đề (Title)
            // Sắp xếp bài mới nhất lên đầu
            var results = await _context.Threads
                .Include(t => t.AppUser).ThenInclude(u => u.UserProfile) // Lấy thông tin người tạo
                .Include(t => t.Forum) // Lấy tên Forum
                .Include(t => t.Posts) // Để đếm số replies
                .Where(t => !t.IsDeleted && t.Title.Contains(q)) // Logic tìm kiếm: Title chứa từ khóa 'q'
                .OrderByDescending(t => t.CreatedAt)
                .Take(50) // Giới hạn 50 kết quả để không bị nặng
                .ToListAsync();

            // Truyền từ khóa ra View để hiển thị lại trong ô input (UX)
            ViewBag.Query = q;

            return View(results);
        }
    }
}