using BlogWebsite.Data;
using BlogWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogWebsite.ViewComponents
{
    public class ForumSelectorViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ForumSelectorViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy danh sách Category và Forum con (để hiển thị trong Modal)
            var items = await _context.Categories
                .Include(c => c.Forums)
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            return View(items);
        }
    }
}