using BlogWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogWebsite.ViewComponents
{
    public class LatestPostsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public LatestPostsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 10)
        {
            // Lấy các thread mới nhất (giống như NewPosts nhưng giới hạn số lượng)
            var threads = await _context.Threads
                .Include(t => t.AppUser)
                    .ThenInclude(u => u.UserProfile)
                .Include(t => t.Forum)
                .Include(t => t.Posts)
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync();

            return View(threads);
        }
    }
}

