namespace BlogWebsite.Areas.Admin.Models
{
    public class DashboardSummaryViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalForums { get; set; }
        public int TotalThreads { get; set; }
        public int TotalPosts { get; set; }
        public int PendingReports { get; set; }
    }
}

