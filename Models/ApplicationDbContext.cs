using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlogWebsite.Models;
namespace BlogWebsite.Models
{
    // Kế thừa từ IdentityDbContext<AppUser> để quản lý cả Identity và các bảng của bạn
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        // Đăng ký các DbSet cho từng Model
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<Thread> Threads { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Report> Reports { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Rất quan trọng, phải gọi đầu tiên

            // Cấu hình liên kết 1-1 cho AppUser và UserProfile
            builder.Entity<AppUser>()
                .HasOne(a => a.UserProfile)
                .WithOne(u => u.AppUser)
                .HasForeignKey<UserProfile>(u => u.UserId);

            // Cấu hình ràng buộc UNIQUE cho Bảng Like
            // (1 người chỉ được like 1 bài 1 lần)
            builder.Entity<Like>()
                .HasIndex(l => new { l.PostId, l.AppUserId })
                .IsUnique();

            // Cấu hình để lưu Enum ReportStatus dưới dạng chuỗi (dễ đọc)
            builder.Entity<Report>()
                .Property(r => r.Status)
                .HasConversion<string>();

            // Cấu hình các mối quan hệ ON DELETE
            // Ngăn chặn "lỗi tham chiếu vòng" (cycles) khi xóa
            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.ClientSetNull;
            }
        }
    }
}
