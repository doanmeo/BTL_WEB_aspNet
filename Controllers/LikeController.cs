using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogWebsite.Models;

namespace BlogWebsite.Controllers
{
    [Authorize] // Chỉ người dùng đã đăng nhập mới có thể Like
    [ApiController] // Đánh dấu đây là một API Controller
    [Route("api/[controller]")] // Định nghĩa route cho API
    public class LikeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public LikeController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/Like/ToggleLike
        [HttpPost("ToggleLike")]
        public async Task<IActionResult> ToggleLike([FromForm] int postId) // Sử dụng [FromForm] vì đây là form POST đơn giản
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Người dùng chưa đăng nhập
            }

            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.IsDeleted)
            {
                return NotFound("Post not found or is deleted.");
            }

            var existingLike = await _context.Likes
                                            .FirstOrDefaultAsync(l => l.PostId == postId && l.AppUserId == userId);

            if (existingLike == null)
            {
                // Người dùng chưa like, thêm like mới
                var newLike = new Like
                {
                    PostId = postId,
                    AppUserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Likes.Add(newLike);
                await _context.SaveChangesAsync();

                // Tạo thông báo cho chủ bài viết (nếu không phải tự like bài mình)
                if (post.AppUserId != userId)
                {
                    var liker = await _userManager.FindByIdAsync(userId);
                    var message = $"{liker?.UserName ?? "Someone"} liked your post.";

                    var notification = new Notification
                    {
                        UserId = post.AppUserId,
                        FromUserId = userId,
                        Type = NotificationType.Like,
                        Message = message,
                        Link = Url.Action("Details", "Thread", new { id = post.ThreadId }),
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }
                // Trả về số lượng like mới
                var likeCount = await _context.Likes.CountAsync(l => l.PostId == postId);
                return Ok(new { liked = true, likeCount = likeCount });
            }
            else
            {
                // Người dùng đã like, xóa like hiện có
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                // Trả về số lượng like mới
                var likeCount = await _context.Likes.CountAsync(l => l.PostId == postId);
                return Ok(new { liked = false, likeCount = likeCount });
            }
        }

        // GET: api/Like/GetLikeCount/5
        [HttpGet("GetLikeCount/{postId}")]
        public async Task<IActionResult> GetLikeCount(int postId)
        {
            var likeCount = await _context.Likes.CountAsync(l => l.PostId == postId);
            var likedByUser = false;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                likedByUser = await _context.Likes.AnyAsync(l => l.PostId == postId && l.AppUserId == userId);
            }
            return Ok(new { likeCount = likeCount, liked = likedByUser });
        }
    }
}
