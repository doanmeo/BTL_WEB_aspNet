using BlogWebsite.Models;

namespace BlogWebsite.Models.ViewModels
{
    public class ForumDetailsViewModel
    {
        public Forum Forum { get; set; } = null!;
        public PagedResult<Thread> Threads { get; set; } = PagedResult<Thread>.Empty();
    }
}

