using BlogWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogWebsite.Data
{
    public class AppDbContext : DbContext {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            base.OnModelCreating(modelBuilder);
            //// Bỏ tiền tố AspNet của các bảng: mặc định
            //foreach (var entityType in builder.Model.GetEntityTypes ()) {
            //    var tableName = entityType.GetTableName ();
            //    if (tableName.StartsWith ("AspNet")) {
            //        entityType.SetTableName (tableName.Substring (6));
            //    }

            // 1. Cấu hình Index cho cột Slug
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug);
        }
        
        public DbSet<Category> Categories { set; get; }



    } 
}