using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogWebsite.Models;

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
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Tìm forum theo id, đồng thời tải kèm danh sách các thread con
            var forum = await _context.Forums
                .Include(f => f.Threads)
                .FirstOrDefaultAsync(m => m.ForumId == id && !m.IsDeleted);

            if (forum == null)
            {
                return NotFound();
            }

            // Lọc các thread đã bị xóa mềm và sắp xếp
            forum.Threads = forum.Threads.Where(t => !t.IsDeleted).OrderByDescending(t => t.UpdatedAt).ToList();

            return View(forum);
        }
    }
}