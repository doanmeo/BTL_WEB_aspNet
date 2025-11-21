namespace BlogWebsite.Areas.Admin.ViewComponents
{
    // Lớp model đơn giản để biểu diễn một mục menu
    // Lớp model đơn giản để biểu diễn một mục menu
    public class AdminMenuItem
    {
        public string Name { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; } = "Index"; // Mặc định là action Index
        public string Area { get; set; } = "Admin";   // Mặc định là area Admin
        public string IconCssClass { get; set; } // Lớp CSS cho icon (ví dụ: mdi mdi-home)
    }
}
