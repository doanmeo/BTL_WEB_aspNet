using BlogWebsite.Models;
using BlogWebsite.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
namespace BlogWebsite.Controllers
{
    [Authorize] // Yêu cầu người dùng phải đăng nhập để đăng bài
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PostController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Lấy thông tin Thread để đảm bảo nó tồn tại và chưa bị khóa/xóa
                var thread = await _context.Threads.FindAsync(model.ThreadId);
                if (thread == null || thread.IsLocked || thread.IsDeleted)
                {
                    // Thêm lỗi vào ModelState và quay lại trang trước đó
                    ModelState.AddModelError(string.Empty, "This thread is not available for replies.");
                    // Bạn cần một cơ chế để quay lại trang thread chi tiết, có thể dùng TempData hoặc trả về View lỗi
                    return RedirectToAction("Details", "Thread", new { id = model.ThreadId });
                }

                var post = new Post
                {
                    Content = model.Content,
                    ThreadId = model.ThreadId,
                    // Lấy UserId của người dùng đang đăng nhập
                    AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    CreatedAt = DateTime.UtcNow,
                    EditedAt = null
                };

                _context.Posts.Add(post);

                // Cập nhật lại ngày UpdatedAt của Thread
                thread.UpdatedAt = DateTime.UtcNow;
                _context.Threads.Update(thread);

                await _context.SaveChangesAsync();

                // Tạo thông báo cho chủ thread (nếu người trả lời khác chủ thread)
                if (thread.AppUserId != post.AppUserId)
                {
                    var replier = await _userManager.FindByIdAsync(post.AppUserId);
                    var message = $"{replier?.UserName ?? "Someone"} replied to your thread \"{thread.Title}\".";

                    var notification = new Notification
                    {
                        UserId = thread.AppUserId,
                        FromUserId = post.AppUserId,
                        Type = NotificationType.Reply,
                        Message = message,
                        Link = Url.Action("Details", "Thread", new { id = thread.ThreadId }),
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }

                // Redirect người dùng về lại trang thread họ vừa trả lời
                return RedirectToAction("Details", "Thread", new { id = model.ThreadId });
            }

            // Nếu model không hợp lệ, quay lại trang thread và hiển thị lỗi
            // Cần một cách để truyền lỗi về lại trang trước. Dùng TempData là một lựa chọn.
            TempData["ErrorMessage"] = "Your reply could not be posted. Please check the errors.";
            return RedirectToAction("Details", "Thread", new { id = model.ThreadId });
        }

        // GET: /Post/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // 1. Tìm bài viết
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            // 2. Kiểm tra quyền (Chỉ chủ bài viết hoặc Admin mới được sửa)
            var currentUser = await _userManager.GetUserAsync(User);
            if (post.AppUserId != currentUser.Id && !User.IsInRole("Admin"))
            {
                return Forbid(); // Trả về lỗi 403 (Không có quyền)
            }

            // 3. Trả về View
            return View(post);
        }
        // POST: /Post/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content)
        {
            // 1. Tìm bài viết gốc
            var postToUpdate = await _context.Posts.FindAsync(id);

            if (postToUpdate == null)
            {
                return NotFound();
            }

            // 2. Kiểm tra quyền lần nữa (Bảo mật)
            var currentUser = await _userManager.GetUserAsync(User);
            if (postToUpdate.AppUserId != currentUser.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // 3. Cập nhật dữ liệu
            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("Content", "Nội dung không được để trống");
                return View(postToUpdate);
            }

            postToUpdate.Content = content;
            // postToUpdate.UpdatedAt = DateTime.Now; // Bỏ comment nếu Model Post có trường UpdatedAt

            // 4. Lưu và quay về trang Thread
            await _context.SaveChangesAsync();

            // Quay về trang chi tiết Thread, nhảy đúng đến vị trí bài viết vừa sửa (dùng fragment #post-id)
            return RedirectToAction("Details", "Thread", new { id = postToUpdate.ThreadId }, $"post-{id}");
        }
    }
}
