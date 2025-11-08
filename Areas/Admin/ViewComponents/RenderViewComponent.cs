using Microsoft.AspNetCore.Mvc;

namespace lab01.ViewComponents
{
    public class RenderViewComponent : ViewComponent
    {
        private List<MenuItem> MenuItems = new List<MenuItem>();

        // Hàm tạo để giả lập dữ liệu menu
        public RenderViewComponent()
        {
            MenuItems = new List<MenuItem>() {
                new MenuItem() { Id = 1, Name = "Branches", Link = "Branches/List" },
                new MenuItem() { Id = 2, Name = "Students", Link = "Admin/Student/List" },
                new MenuItem() { Id = 3, Name = "Subjects", Link = "Subjects/List" },
                new MenuItem() { Id = 4, Name = "Courses", Link = "Courses/List" }
            };
        }

        // Hàm InvokeAsync sẽ được gọi khi bạn @await Component.InvokeAsync...
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Trả về một View tên là "RenderLeftMenu" và truyền danh sách MenuItems làm Model
            return View("~/Views/Shared/Components/Render/RenderLeftMenu.cshtml", MenuItems);
        }
    }
}
