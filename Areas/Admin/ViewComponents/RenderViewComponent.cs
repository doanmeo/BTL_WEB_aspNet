using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlogWebsite.Areas.Admin.ViewComponents
{
    // Lớp model đơn giản để biểu diễn một mục menu cho Admin Sidebar
    
    public class RenderViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Tạo danh sách các mục menu cho sidebar (left menu) của Admin
            var menuItems = new List<AdminMenuItem>
            {
                new AdminMenuItem { Name = "Dashboard", Controller = "Dashboard", IconCssClass = "mdi mdi-home" },
                new AdminMenuItem { Name = "Categories", Controller = "Categories", IconCssClass = "mdi mdi-format-list-bulleted" },
                new AdminMenuItem { Name = "Forums", Controller = "Forums", IconCssClass = "mdi mdi-comment-multiple-outline" },
                new AdminMenuItem { Name = "Users", Controller = "Users", IconCssClass = "mdi mdi-account-multiple" },
                new AdminMenuItem { Name = "Reports", Controller = "Report", IconCssClass = "mdi mdi-flag" }
            };

            // Render Partial View _RenderLeftMenu.cshtml và truyền danh sách menu cho nó.
            return View("~/Areas/Admin/Views/Shared/Render/_RenderLeftMenu.cshtml", menuItems);
        }
    }
}
