using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogWebsite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BlogWebsite.Models.ViewModels;
using Thread = BlogWebsite.Models.Thread;

namespace BlogWebsite.Controllers
{
    public class ThreadController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ThreadController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Thread/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var thread = await _context.Threads
                .Include(t => t.Forum) // Lấy thông tin Forum để hiển thị breadcrumb
                .Include(t => t.Posts)
                    .ThenInclude(p => p.AppUser)
                    .ThenInclude(au => au.UserProfile)
                .FirstOrDefaultAsync(m => m.ThreadId == id && !m.IsDeleted);

            if (thread == null) return NotFound();

            thread.Posts = thread.Posts.Where(p => !p.IsDeleted).OrderBy(p => p.CreatedAt).ToList();

            thread.ViewCount++;
            _context.Update(thread);
            await _context.SaveChangesAsync();

            return View(thread);
        }

        // GET: Thread/Create?forumId=5
        [Authorize]
        public async Task<IActionResult> Create(int forumId)
        {
            var forum = await _context.Forums.FirstOrDefaultAsync(f => f.ForumId == forumId && !f.IsDeleted);
            if (forum == null)
            {
                return NotFound("The specified forum does not exist.");
            }

            // Tạo ViewModel và gán ForumId
            var model = new ThreadViewModel
            {
                ForumId = forumId
            };

            ViewBag.ForumName = forum.Name; // Gửi tên Forum qua ViewBag để hiển thị trên View
            return View(model);
        }

        // POST: Thread/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThreadViewModel model)
        {
            var forum = await _context.Forums.FirstOrDefaultAsync(f => f.ForumId == model.ForumId && !f.IsDeleted);
            if (forum == null)
            {
                ModelState.AddModelError("ForumId", "The specified forum does not exist.");
            }

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var now = DateTime.UtcNow;

                // Bước 1: Tạo đối tượng Thread
                var newThread = new Thread
                {
                    ForumId = model.ForumId,
                    Title = model.Title,
                    AppUserId = userId,
                    CreatedAt = now,
                    UpdatedAt = now, // Khi mới tạo, UpdatedAt = CreatedAt
                    ViewCount = 0,
                    IsLocked = false
                };

                // Bước 2: Tạo bài Post đầu tiên cho Thread đó
                var firstPost = new Post
                {
                    Content = model.Content,
                    AppUserId = userId,
                    CreatedAt = now,
                    Thread = newThread // Gán trực tiếp Thread mới tạo vào Post
                };

                // Bước 3: Thêm cả hai vào DbContext và lưu lại
                // Entity Framework đủ thông minh để xử lý việc này
                _context.Posts.Add(firstPost);
                await _context.SaveChangesAsync();

                // Chuyển hướng người dùng đến trang chi tiết của thread vừa tạo
                return RedirectToAction("Details", new { id = newThread.ThreadId });
            }

            // Nếu model không hợp lệ, trả về lại form Create
            ViewBag.ForumName = forum?.Name ?? "Unknown";
            return View(model);
        }
    }
}
