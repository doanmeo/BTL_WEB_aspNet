using System.Threading.Tasks;
using BlogWebsite.Areas.Admin.Models;
using BlogWebsite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogWebsite.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            ViewData["ReturnUrl"] = GetReturnUrl(returnUrl);
            return View(new AdminLoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = GetReturnUrl(returnUrl);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không hợp lệ.");
                return View(model);
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (signInResult.Succeeded)
            {
                var destination = GetReturnUrl(returnUrl);
                return Redirect(destination);
            }

            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa tạm thời.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không hợp lệ.");
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        private string GetReturnUrl(string? returnUrl)
        {
            var fallback = Url.Action("Index", "Dashboard", new { area = "Admin" }) ?? "/Admin/Dashboard";
            if (string.IsNullOrEmpty(returnUrl))
            {
                return fallback;
            }

            return Url.IsLocalUrl(returnUrl) ? returnUrl : fallback;
        }
    }
}

