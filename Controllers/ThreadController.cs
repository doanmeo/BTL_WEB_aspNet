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

                // BƯỚC 2: TẠO POST ĐẦU TIÊN (Lưu Nội dung)
                // Chuyển đổi xuống dòng (\n) thành thẻ <br> nếu dùng textarea thường
                string contentSafe = model.Content.Replace("\n", "<br>");

                var post = new Post
                {
                    ThreadId = thread.ThreadId, // <--- Gắn vào Thread vừa tạo ở trên
                    Content = contentSafe,      // <--- Nội dung vào đây
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
        public async Task<IActionResult> NewPosts(string searchQuery, int page = 1)
        {
            int pageSize = 20; // Số bài mỗi trang

            // 1. Query cơ bản (chưa chạy)
            var query = _context.Threads
                .Include(t => t.AppUser).ThenInclude(u => u.UserProfile)
                .Include(t => t.Forum)
                .Include(t => t.Posts)
                .Where(t => !t.IsDeleted);

            // 2. Lọc theo nội dung bài viết (nếu có)
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(t => t.Posts.Any(p => p.Content.Contains(searchQuery)));
            }

            // 3. Sắp xếp mới nhất
            query = query.OrderByDescending(t => t.CreatedAt);

            // 4. Tính toán phân trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Đảm bảo page hợp lệ
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            // 5. Lấy dữ liệu trang hiện tại
            var threads = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 6. Truyền thông tin phân trang ra View
            // Thay vì dùng ViewBag, chúng ta sẽ chuyển sang ViewModel ở các bước tiếp theo
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchQuery = searchQuery; // Để điền lại vào ô input

            return View(threads);
        }

        // GET: /Thread/WhatsNew - dùng cho tab "What's new" (trending)
        [HttpGet]
        public async Task<IActionResult> WhatsNew()
        {
            // Lấy các thread có nhiều replies nhất (giống Trending)
            var threadData = await _context.Threads
                .Where(t => !t.IsDeleted)
                .Select(t => new
                {
                    Thread = t,
                    PostCount = t.Posts.Count(p => !p.IsDeleted)
                })
                .Where(x => x.PostCount > 1)
                .OrderByDescending(x => x.PostCount)
                .ThenByDescending(x => x.Thread.UpdatedAt ?? x.Thread.CreatedAt)
                .Take(20)
                .ToListAsync();

            var threadIds = threadData.Select(x => x.Thread.ThreadId).ToList();

            var threads = await _context.Threads
                .Include(t => t.AppUser).ThenInclude(u => u.UserProfile)
                .Include(t => t.Forum)
                .Include(t => t.Posts)
                .Where(t => threadIds.Contains(t.ThreadId))
                .ToListAsync();

            // Giữ nguyên thứ tự theo PostCount
            var dict = threads.ToDictionary(t => t.ThreadId);
            var orderedThreads = threadIds
                .Where(id => dict.ContainsKey(id))
                .Select(id => dict[id])
                .ToList();

            return View(orderedThreads);
        }

        // GET: /Thread/MyPosts - tab "My posts" (trước là New profile posts)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyPosts()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var threads = await _context.Threads
                .Include(t => t.AppUser).ThenInclude(u => u.UserProfile)
                .Include(t => t.Forum)
                .Include(t => t.Posts)
                .Where(t => !t.IsDeleted && t.AppUserId == userId)
                .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
                .Take(20)
                .ToListAsync();

            return View(threads);
        }
        // GET: /Thread/Watched
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Watched()
        {
            var userId = _userManager.GetUserId(User);

            var watchedThreadIds = await _context.WatchedThreads
                .Where(w => w.AppUserId == userId)
                .Select(w => w.ThreadId)
                .ToListAsync();

            var threads = await _context.Threads
                .Include(t => t.AppUser).ThenInclude(u => u.UserProfile)
                .Include(t => t.Forum)
                .Include(t => t.Posts)
                .Where(t => watchedThreadIds.Contains(t.ThreadId) && !t.IsDeleted)
                .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
                .ToListAsync();

            return View(threads);
        }

        // POST: /Thread/ToggleWatch/5
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleWatch(int id)
        {
            var userId = _userManager.GetUserId(User);
            var thread = await _context.Threads.FindAsync(id);

            if (thread == null)
            {
                return NotFound(new { success = false, message = "Thread not found." });
            }

            var existingWatch = await _context.WatchedThreads
                .FirstOrDefaultAsync(w => w.ThreadId == id && w.AppUserId == userId);

            if (existingWatch != null)
            {
                // Đã watch -> Bỏ watch
                _context.WatchedThreads.Remove(existingWatch);
                await _context.SaveChangesAsync();
                return Json(new { success = true, watching = false, message = "Successfully unwatched." });
            }
            else
            {
                // Chưa watch -> Thêm watch
                var newWatch = new WatchedThread
                {
                    AppUserId = userId,
                    ThreadId = id,
                    WatchedAt = DateTime.UtcNow
                };
                _context.WatchedThreads.Add(newWatch);
                await _context.SaveChangesAsync();
                return Json(new { success = true, watching = true, message = "Successfully watched." });
            }
        }
    }
}
