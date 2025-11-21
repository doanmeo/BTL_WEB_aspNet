using System.Linq;
using System.Threading.Tasks;
using BlogWebsite.Areas.Admin.Models;
using BlogWebsite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogWebsite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var summary = new DashboardSummaryViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalForums = await _context.Forums.CountAsync(),
                TotalThreads = await _context.Threads.CountAsync(),
                TotalPosts = await _context.Posts.CountAsync(),
                PendingReports = await _context.Reports.CountAsync(r => r.Status == ReportStatus.Pending)
            };

            return View(summary);
        }
    }
}

