namespace BlogWebsite.Models
{
    public enum ReportStatus
    {
        Pending = 0,  // Mới (chờ xử lý)
        Handled = 1,  // Đã xử lý
        Ignored = 2   // Bỏ qua
    }
}
