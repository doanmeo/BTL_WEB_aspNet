using Microsoft.AspNetCore.Mvc;

namespace BlogWebsite.ViewComponents
{
    public class MainFooterViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Có thể thêm logic ở đây nếu cần
            return View();
        }
    }
}
