using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogWebsite.Models;
using BlogWebsite.Models.ViewModels;
using ThreadEntity = BlogWebsite.Models.Thread;

namespace BlogWebsite.Controllers
{
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ForumController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Forum/Details/5
        public async Task<IActionResult> Details(int? id, int page = 1)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forum = await _context.Forums
                .Include(f => f.Category)
                .FirstOrDefaultAsync(m => m.ForumId == id && !m.IsDeleted);

            if (forum == null)
            {
                return NotFound();
            }

            var threadsQuery = _context.Threads
                .Where(t => t.ForumId == forum.ForumId && !t.IsDeleted)
                .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt);

            var pagedThreads = await PagedResult<ThreadEntity>.CreateAsync(threadsQuery, page, 12);

            var viewModel = new ForumDetailsViewModel
            {
                Forum = forum,
                Threads = pagedThreads
            };

            return View(viewModel);
        }
    }
}