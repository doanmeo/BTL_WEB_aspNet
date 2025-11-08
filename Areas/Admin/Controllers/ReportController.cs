using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogWebsite.Models;
using Microsoft.AspNetCore.Authorization;

namespace BlogWebsite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Report
        public async Task<IActionResult> Index()
        {
            var reports = await _context.Reports
                                        .Include(r => r.Post)
                                        .ThenInclude(p => p.AppUser)
                                        .Include(r => r.AppUser)
                                        .OrderBy(r => r.Status)
                                        .ThenByDescending(r => r.CreatedAt)
                                        .ToListAsync();
            return View(reports);
        }

        // POST: Admin/Report/Process/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            // SỬA LỖI: Đổi Processed thành Handled
            report.Status = ReportStatus.Handled;
            _context.Update(report);

            var post = await _context.Posts.FindAsync(report.PostId);
            if (post != null)
            {
                post.IsDeleted = true;
                _context.Update(post);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Report processed successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Report/Ignore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ignore(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            report.Status = ReportStatus.Ignored;
            _context.Update(report);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Report ignored.";
            return RedirectToAction(nameof(Index));
        }
    }
}
