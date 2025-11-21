using BlogWebsite.Models;

namespace BlogWebsite.Models.ViewModels
{
    public class ThreadDetailsViewModel
    {
        public Thread Thread { get; set; } = null!;
        public PagedResult<Post> Posts { get; set; } = PagedResult<Post>.Empty();
    }
}

