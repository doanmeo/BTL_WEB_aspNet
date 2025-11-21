using System;
using System.Linq;
using System.Threading.Tasks;
using BlogWebsite.Areas.Admin.Models;
using BlogWebsite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using BlogWebsite.Models.ViewModels;

namespace BlogWebsite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ForumsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ForumsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var query = _context.Forums
                .Include(f => f.Category)
                .Include(f => f.AppUser)
                .Include(f => f.Threads)
                .Where(f => !f.IsDeleted)
                .OrderBy(f => f.Name);

            var pagedForums = await PagedResult<Forum>.CreateAsync(query, page, 9);
            return View(pagedForums);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var forum = await _context.Forums
                .Include(f => f.Category)
                .Include(f => f.AppUser)
                .FirstOrDefaultAsync(f => f.ForumId == id);
            if (forum == null) return NotFound();
            return View(forum);
        }

        public async Task<IActionResult> Create()
        {
            var vm = await BuildFormViewModelAsync();
            // default owner = current user if available
            vm.AppUserId = _userManager.GetUserId(User) ?? vm.UserOptions.FirstOrDefault()?.Value;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminForumFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await BuildFormViewModelAsync(model);
                return View(model);
            }

            var forum = new Forum
            {
                Name = model.Name,
                Description = model.Description,
                CategoryId = model.CategoryId!.Value,
                AppUserId = model.AppUserId!,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = model.IsDeleted
            };

            _context.Forums.Add(forum);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var forum = await _context.Forums.FindAsync(id);
            if (forum == null) return NotFound();

            var vm = await BuildFormViewModelAsync(new AdminForumFormViewModel
            {
                ForumId = forum.ForumId,
                Name = forum.Name,
                Description = forum.Description,
                CategoryId = forum.CategoryId,
                AppUserId = forum.AppUserId,
                IsDeleted = forum.IsDeleted
            });
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminForumFormViewModel model)
        {
            if (id != model.ForumId) return NotFound();
            if (!ModelState.IsValid)
            {
                model = await BuildFormViewModelAsync(model);
                return View(model);
            }

            var forum = await _context.Forums.FindAsync(id);
            if (forum == null) return NotFound();

            forum.Name = model.Name;
            forum.Description = model.Description;
            forum.CategoryId = model.CategoryId!.Value;
            forum.AppUserId = model.AppUserId!;
            forum.IsDeleted = model.IsDeleted;
            forum.UpdatedAt = DateTime.UtcNow;

            _context.Forums.Update(forum);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = forum.ForumId });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var forum = await _context.Forums
                .Include(f => f.Category)
                .Include(f => f.AppUser)
                .FirstOrDefaultAsync(f => f.ForumId == id);
            if (forum == null) return NotFound();
            return View(forum);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var forum = await _context.Forums.FindAsync(id);
            if (forum == null) return NotFound();

            forum.IsDeleted = true;
            forum.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<AdminForumFormViewModel> BuildFormViewModelAsync(AdminForumFormViewModel? preset = null)
        {
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();

            var users = await _userManager.Users
                .Select(u => new SelectListItem { Value = u.Id, Text = u.UserName })
                .ToListAsync();

            var vm = preset ?? new AdminForumFormViewModel();
            vm.CategoryOptions = categories;
            vm.UserOptions = users;
            return vm;
        }
    }
}

