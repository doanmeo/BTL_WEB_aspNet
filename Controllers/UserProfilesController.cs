using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogWebsite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BlogWebsite.Models.ViewModels; // Thêm ViewModel
using System.Security.Claims;

namespace BlogWebsite.Controllers
{
    [Authorize] // Yêu cầu đăng nhập để xem và chỉnh sửa hồ sơ
    public class UserProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserProfilesController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UserProfiles/Details/some-user-id (đã có từ trước)
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return NotFound("User not found.");
                id = currentUser.Id;
            }

            var userProfile = await _context.UserProfiles
                .Include(up => up.User)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (userProfile == null)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    userProfile = new UserProfile
                    {
                        UserId = user.Id,
                        DisplayName = user.UserName,
                        User = user,
                        JoinedAt = DateTime.UtcNow // Đảm bảo có giá trị mặc định
                    };
                    // Nếu profile chưa tồn tại, có thể tạo mới ở đây hoặc cho phép tạo khi Edit lần đầu
                    // Ở đây ta chỉ tạo tạm thời để View không bị lỗi
                }
                else
                {
                    return NotFound("The requested user profile was not found.");
                }
            }

            return View(userProfile);
        }

        // GET: UserProfiles/MyProfile - Hiển thị form chỉnh sửa hồ sơ của chính người dùng
        public async Task<IActionResult> MyProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not logged in.");
            }

            var userProfile = await _context.UserProfiles
                                        .FirstOrDefaultAsync(up => up.UserId == currentUser.Id);

            if (userProfile == null)
            {
                // Nếu người dùng chưa có profile, tạo một profile mới với giá trị mặc định
                userProfile = new UserProfile
                {
                    UserId = currentUser.Id,
                    DisplayName = currentUser.UserName,
                    JoinedAt = DateTime.UtcNow,
                    Reputation = 0
                };
                _context.UserProfiles.Add(userProfile);
                await _context.SaveChangesAsync();
            }

            // Chuyển đổi từ UserProfile Model sang EditProfileViewModel để hiển thị form
            var model = new EditProfileViewModel
            {
                DisplayName = userProfile.DisplayName,
                AvatarUrl = userProfile.AvatarUrl,
                Bio = userProfile.Bio
            };
            return View("Edit", model); // Sử dụng cùng View Edit.cshtml
        }

        // POST: UserProfiles/Edit - Xử lý việc gửi form chỉnh sửa hồ sơ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index", "Home"); // Hoặc trang lỗi
                }

                var userProfile = await _context.UserProfiles
                                        .FirstOrDefaultAsync(up => up.UserId == currentUser.Id);

                if (userProfile == null)
                {
                    // Đây là trường hợp hiếm nếu đã xử lý ở MyProfile, nhưng vẫn nên có
                    userProfile = new UserProfile
                    {
                        UserId = currentUser.Id,
                        DisplayName = model.DisplayName ?? currentUser.UserName,
                        JoinedAt = DateTime.UtcNow,
                        Reputation = 0
                    };
                    _context.UserProfiles.Add(userProfile);
                }

                // Cập nhật các trường từ ViewModel vào Model CSDL
                userProfile.DisplayName = model.DisplayName;
                userProfile.AvatarUrl = model.AvatarUrl;
                userProfile.Bio = model.Bio;

                _context.Update(userProfile);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Details", new { id = currentUser.Id }); // Chuyển hướng về trang profile chi tiết
            }

            // Nếu model không hợp lệ, quay lại form với lỗi
            return View(model);
        }

        // Dành cho Admin xem danh sách (tùy chọn, đã có từ trước)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var profiles = await _context.UserProfiles.Include(p => p.User).ToListAsync();
            return View(profiles);
        }
    }
}
