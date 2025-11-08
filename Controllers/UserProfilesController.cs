using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogWebsite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BlogWebsite.Controllers
{
    [Authorize] // Yêu cầu đăng nhập để xem hồ sơ
    public class UserProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserProfilesController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UserProfiles/Details/some-user-id
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                // Nếu không có id, xem hồ sơ của chính mình
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return NotFound("User not found.");
                }
                id = currentUser.Id;
            }

            var userProfile = await _context.UserProfiles
                .Include(up => up.User) // Lấy thông tin từ AppUser (ví dụ: UserName, Email)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (userProfile == null)
            {
                // Có thể người dùng này chưa có profile, ta tạo một profile trống
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    userProfile = new UserProfile
                    {
                        UserId = user.Id,
                        DisplayName = user.UserName,
                        User = user
                    };
                    // Không lưu vào DB, chỉ hiển thị tạm thời
                }
                else
                {
                    return NotFound("The requested user profile was not found.");
                }
            }

            return View(userProfile);
        }

        // Dành cho Admin xem danh sách (tùy chọn, có thể thêm sau)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var profiles = await _context.UserProfiles.Include(p => p.User).ToListAsync();
            return View(profiles);
        }
    }
}
