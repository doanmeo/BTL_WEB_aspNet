using BlogWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogWebsite.ViewComponents
{
    public class TrendingPostsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public TrendingPostsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 5)
        {
            // Lấy các thread có nhiều replies nhất
            // Số replies = số Posts - 1 (post đầu tiên không tính là reply)
            // Sử dụng subquery để đếm số posts hiệu quả hơn
            var threadData = await _context.Threads
                .Where(t => !t.IsDeleted)
                .Select(t => new
                {
                    ThreadId = t.ThreadId,
                    PostCount = t.Posts.Count(p => !p.IsDeleted)
                })
                .Where(x => x.PostCount > 1) // Chỉ lấy thread có ít nhất 1 reply
                .OrderByDescending(x => x.PostCount)
                .Take(count)
                .ToListAsync();

            var threadIds = threadData.Select(x => x.ThreadId).ToList();

            if (!threadIds.Any())
            {
                return View(new List<Models.Thread>());
            }

            // Load đầy đủ thông tin cho các thread đã được sắp xếp
            var threads = await _context.Threads
                .Include(t => t.AppUser)
                    .ThenInclude(u => u.UserProfile)
                .Include(t => t.Forum)
                .Include(t => t.Posts)
                .Where(t => threadIds.Contains(t.ThreadId))
                .ToListAsync();

            // Lọc posts đã bị xóa và sắp xếp lại theo thứ tự đã lấy từ query trên
            foreach (var thread in threads)
            {
                if (thread.Posts != null)
                {
                    thread.Posts = thread.Posts.Where(p => !p.IsDeleted).ToList();
                }
            }

            // Sắp xếp lại theo thứ tự đã lấy từ query trên (giữ nguyên thứ tự)
            var threadDict = threads.ToDictionary(t => t.ThreadId);
            var orderedThreads = threadIds
                .Select(id => threadDict.ContainsKey(id) ? threadDict[id] : null)
                .Where(t => t != null)
                .ToList();

            return View(orderedThreads);
        }
    }
}

