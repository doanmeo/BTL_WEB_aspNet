using System;
using System.Linq;
using System.Threading.Tasks;
using BlogWebsite.Areas.Admin.Models;
using BlogWebsite.Models;
using BlogWebsite.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogWebsite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var query = _context.Users
                .Include(u => u.UserProfile)
                .OrderBy(u => u.UserName);

            var pagedUsers = await PagedResult<AppUser>.CreateAsync(query, page, 12);
            return View(pagedUsers);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            var vm = MapToViewModel(user);
            return View(vm);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            return View(MapToViewModel(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AdminUserEditViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.EmailConfirmed = model.EmailConfirmed;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var profile = user.UserProfile ?? new UserProfile { UserId = user.Id, JoinedAt = DateTime.UtcNow };
            profile.DisplayName = model.DisplayName;
            profile.AvatarUrl = model.AvatarUrl;
            profile.Bio = model.Bio;

            if (user.UserProfile == null)
            {
                _context.UserProfiles.Add(profile);
            }
            else
            {
                _context.UserProfiles.Update(profile);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật người dùng thành công.";
            return RedirectToAction(nameof(Details), new { id = user.Id });
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            return View(MapToViewModel(user));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            if (user.UserProfile != null)
            {
                _context.UserProfiles.Remove(user.UserProfile);
            }

            var userRoles = await _context.UserRoles.Where(r => r.UserId == id).ToListAsync();
            if (userRoles.Any())
            {
                _context.UserRoles.RemoveRange(userRoles);
            }

            var userLogins = await _context.UserLogins.Where(l => l.UserId == id).ToListAsync();
            if (userLogins.Any())
            {
                _context.UserLogins.RemoveRange(userLogins);
            }

            var userTokens = await _context.UserTokens.Where(t => t.UserId == id).ToListAsync();
            if (userTokens.Any())
            {
                _context.UserTokens.RemoveRange(userTokens);
            }

            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Không thể xóa người dùng.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Đã xóa người dùng.";
            return RedirectToAction(nameof(Index));
        }

        private static AdminUserEditViewModel MapToViewModel(AppUser user)
        {
            return new AdminUserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                DisplayName = user.UserProfile?.DisplayName,
                AvatarUrl = user.UserProfile?.AvatarUrl,
                Bio = user.UserProfile?.Bio
            };
        }
    }
}

