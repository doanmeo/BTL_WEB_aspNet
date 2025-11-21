using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogWebsite.Models;
using Microsoft.AspNetCore.Authorization;
using BlogWebsite.Models.ViewModels;

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
        public async Task<IActionResult> Index(ReportStatus? statusFilter, int page = 1)
        {
            var query = _context.Reports
                                .Include(r => r.Post)
                                .ThenInclude(p => p.AppUser)
                                .Include(r => r.AppUser)
                                .OrderBy(r => r.Status)
                                .ThenByDescending(r => r.CreatedAt)
                                .AsQueryable();

            if (statusFilter.HasValue)
            {
                query = query.Where(r => r.Status == statusFilter);
            }

            ViewData["StatusFilter"] = statusFilter;
            var totalReports = await _context.Reports.CountAsync();
            var pendingReports = await _context.Reports.CountAsync(r => r.Status == ReportStatus.Pending);
            var handledReports = await _context.Reports.CountAsync(r => r.Status == ReportStatus.Handled);
            var ignoredReports = await _context.Reports.CountAsync(r => r.Status == ReportStatus.Ignored);

            ViewBag.ReportStats = new
            {
                Total = totalReports,
                Pending = pendingReports,
                Handled = handledReports,
                Ignored = ignoredReports
            };

            var pagedReports = await PagedResult<Report>.CreateAsync(query, page, 12);
            return View(pagedReports);
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
