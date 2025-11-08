using System.Diagnostics;
using BlogWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Thêm using cho Entity Framework

namespace BlogWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Thêm DbContext

        // Tiêm ApplicationDbContext vào HomeController
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Truy vấn lấy tất cả Categories, và mỗi Category thì kèm theo Forums con
            var categories = await _context.Categories
                                        .Include(c => c.Forums)
                                        .Where(c => !c.IsDeleted) // Chỉ lấy category chưa xóa
                                        .ToListAsync();

            // Lọc ra các forum chưa bị xóa mềm
            foreach (var category in categories)
            {
                // Lấy các forum chưa bị xóa và sắp xếp theo tên
                category.Forums = category.Forums.Where(f => !f.IsDeleted).OrderBy(f => f.Name).ToList();
            }

            return View(categories); // Gửi danh sách categories đến View
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
