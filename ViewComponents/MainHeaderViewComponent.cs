using Microsoft.AspNetCore.Mvc;

namespace BlogWebsite.ViewComponents
{
    public class MainHeaderViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Có thể thêm logic ở đây nếu cần, ví dụ: lấy dữ liệu động cho menu
            return View();
        }
    }
}
