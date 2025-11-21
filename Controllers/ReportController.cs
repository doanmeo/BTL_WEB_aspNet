using BlogWebsite.Models;
using BlogWebsite.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogWebsite.Controllers
{
    [Authorize] // Chỉ người dùng đã đăng nhập mới có thể gửi báo cáo
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ReportController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Report/SubmitReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReport(ReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(); // Người dùng chưa đăng nhập
                }

                var post = await _context.Posts.FindAsync(model.PostId);
                if (post == null || post.IsDeleted)
                {
                    return NotFound("Post not found or is deleted.");
                }

                // Kiểm tra xem người dùng đã báo cáo bài này chưa (tùy chọn, để tránh spam)
                var existingReport = await _context.Reports
                                                .FirstOrDefaultAsync(r => r.PostId == model.PostId && r.AppUserId == userId && r.Status == ReportStatus.Pending);
                if (existingReport != null)
                {
                    return Conflict("You have already reported this post."); // Trả về 409 Conflict
                }

                var report = new Report
                {
                    PostId = model.PostId,
                    AppUserId = userId,
                    Reason = model.Reason,
                    CreatedAt = DateTime.UtcNow,
                    Status = ReportStatus.Pending // Báo cáo mới luôn ở trạng thái Pending
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                return Ok("Report submitted successfully."); // Trả về 200 OK
            }

            return BadRequest(ModelState); // Trả về lỗi validation nếu model không hợp lệ
        }
    }
}
