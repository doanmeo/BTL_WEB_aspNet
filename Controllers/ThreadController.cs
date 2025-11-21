using BlogWebsite.Models;
using BlogWebsite.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
        public async Task<IActionResult> Details(int? id, int page = 1)
        {
            if (id == null) return NotFound();

            var thread = await _context.Threads
                .Include(t => t.Forum)
                .Include(t => t.AppUser)
                    .ThenInclude(u => u.UserProfile)
                .FirstOrDefaultAsync(m => m.ThreadId == id && !m.IsDeleted);

            if (thread == null) return NotFound();

            thread.ViewCount++;
            _context.Update(thread);
            await _context.SaveChangesAsync();

            var postsQuery = _context.Posts
                .Where(p => p.ThreadId == thread.ThreadId && !p.IsDeleted)
                .Include(p => p.AppUser)
                    .ThenInclude(u => u.UserProfile)
                .OrderBy(p => p.CreatedAt);

            var pagedPosts = await PagedResult<Post>.CreateAsync(postsQuery, page, 10);

            var viewModel = new ThreadDetailsViewModel
            {
                Thread = thread,
                Posts = pagedPosts
            };

            return View(viewModel);
        }

        // GET: Hiển thị form đăng bài
        [Authorize] // Bắt buộc đăng nhập mới được đăng
        [HttpGet]
        public async Task<IActionResult> Create(int forumId)
        {
            var forum = await _context.Forums.FindAsync(forumId);
            if (forum == null) return NotFound();

            // Truyền ForumId sang View để tí nữa Post lên biết bài thuộc forum nào
            var model = new CreateThreadViewModel { ForumId = forumId, ForumName = forum.Name };
            return View(model);
        }

        // POST: Thread/Create
        // POST: Xử lý lưu bài viết
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateThreadViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);

                // BƯỚC 1: TẠO THREAD (Chỉ lưu Tiêu đề)
                var thread = new Thread
                {
                    Title = model.Title, // <--- Tiêu đề vào đây
                    ForumId = model.ForumId,
                    AppUserId = userId,
                    CreatedAt = DateTime.Now,
                    ViewCount = 0,
                    IsDeleted = false,
                    IsLocked = false
                };

                // Lưu Thread trước để lấy được ThreadId
                _context.Threads.Add(thread);
                await _context.SaveChangesAsync();

                // BƯỚC 2: TẠO POST ĐẦU TIÊN (Lưu Nội dung HTML từ trình soạn thảo)
                var contentSafe = model.Content?.Trim() ?? string.Empty;

                var post = new Post
                {
                    ThreadId = thread.ThreadId, // <--- Gắn vào Thread vừa tạo ở trên
                    Content = contentSafe,      // <--- Nội dung HTML từ Quill
                    AppUserId = userId,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                // 3. Chuyển hướng về trang chi tiết
                return RedirectToAction("Details", new { id = thread.ThreadId });
            }

            // Nếu lỗi thì hiện lại form
            return View(model);
        }
        // GET: /Thread/NewPosts
        [HttpGet]
        public async Task<IActionResult> NewPosts(int page = 1)
        {
            var query = _context.Threads
                .Include(t => t.AppUser).ThenInclude(u => u.UserProfile)
                .Include(t => t.Forum)
                .Include(t => t.Posts)
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt);

            var pagedThreads = await PagedResult<Thread>.CreateAsync(query, page, 20);
            return View(pagedThreads);
        }
    }
}
