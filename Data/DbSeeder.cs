using System;
using System.Linq;
using BlogWebsite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // Cần cái này để dùng IServiceProvider

namespace BlogWebsite.Data
{
    public static class DbSeeder
    {
        // Đổi tên hàm thành SeedAllAsync để gọi 1 lần là xong hết
        public static async Task SeedAllAsync(IServiceProvider service)
        {
            // 1. Lấy các dịch vụ cần thiết
            var context = service.GetRequiredService<ApplicationDbContext>();
            var userManager = service.GetRequiredService<UserManager<AppUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();

            // ==========================================
            // PHẦN 1: TẠO ROLE & ADMIN (Code cũ của bạn)
            // ==========================================

            // Tạo Role Admin/User nếu chưa có
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            // Tạo tài khoản Admin mặc định
            var adminEmail = "admin@gmail.com"; // Bạn có thể đổi email này
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser != null && !IsValidPasswordHash(adminUser.PasswordHash))
            {
                // Nếu mật khẩu đang lưu không hợp lệ (bị sửa tay...), reset lại về mặc định
                await userManager.RemovePasswordAsync(adminUser);
                var resetResult = await userManager.AddPasswordAsync(adminUser, "Admin@123");
                if (!resetResult.Succeeded)
                {
                    throw new InvalidOperationException("Không thể khôi phục mật khẩu cho tài khoản admin mặc định.");
                }
            }

            if (adminUser == null)
            {
                var newAdminUser = new AppUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                // Mật khẩu mặc định là Admin@123
                var result = await userManager.CreateAsync(newAdminUser, "Admin@123");

                if (!result.Succeeded)
                {
                    var reasons = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Không thể tạo tài khoản admin mặc định: {reasons}");
                }

                await userManager.AddToRoleAsync(newAdminUser, "Admin");

                // Tạo UserProfile cho admin
                var userProfile = new UserProfile
                {
                    UserId = newAdminUser.Id,
                    DisplayName = "Administrator",
                    AvatarUrl = "https://via.placeholder.com/150", // Thêm cái ảnh giả cho đẹp
                    Bio = "Quản trị viên hệ thống",
                };
                context.UserProfiles.Add(userProfile);
                await context.SaveChangesAsync();

                // Gán lại adminUser để dùng ở phần dưới
                adminUser = newAdminUser;
            }

            if (adminUser == null)
            {
                throw new InvalidOperationException("Không thể xác định tài khoản admin mặc định sau khi khởi tạo.");
            }

            // ==========================================
            // PHẦN 2: TẠO CATEGORY & FORUM (Code mới của tôi)
            // ==========================================

            // Kiểm tra nếu đã có Category thì thôi, không thêm nữa
            if (context.Categories.Any()) return;

            // Lấy ID của Admin vừa tạo/tìm thấy ở trên để gán quyền sở hữu
            string adminId = adminUser.Id;

            // --- NHÓM 1: MÁY TÍNH ---
            var catMayTinh = new Category { Name = "Máy tính", Description = "Thảo luận về phần cứng, máy tính" };
            context.Categories.Add(catMayTinh);
            await context.SaveChangesAsync(); // Dùng await cho an toàn

            context.Forums.AddRange(new List<Forum>
            {
                new Forum { Name = "Tư vấn cấu hình", Description = "Tư vấn build PC, nâng cấp máy tính", CategoryId = catMayTinh.CategoryId, AppUserId = adminId },
                new Forum { Name = "Overclocking & Cooling & Modding", Description = "Ép xung, tản nhiệt, độ case", CategoryId = catMayTinh.CategoryId, AppUserId = adminId },
                new Forum { Name = "AMD", Description = "Thảo luận về CPU, GPU của AMD", CategoryId = catMayTinh.CategoryId, AppUserId = adminId },
                new Forum { Name = "Intel", Description = "Thảo luận về CPU, Chipset Intel", CategoryId = catMayTinh.CategoryId, AppUserId = adminId },
                new Forum { Name = "GPU & Màn hình", Description = "Card đồ họa và thiết bị hiển thị", CategoryId = catMayTinh.CategoryId, AppUserId = adminId },
                new Forum { Name = "Phần cứng chung", Description = "Các vấn đề phần cứng khác", CategoryId = catMayTinh.CategoryId, AppUserId = adminId },
                new Forum { Name = "Thiết bị ngoại vi & Phụ kiện & Mạng", Description = "Phím, chuột, tai nghe, router...", CategoryId = catMayTinh.CategoryId, AppUserId = adminId },
                new Forum { Name = "Máy tính xách tay", Description = "Laptop, Ultrabook", CategoryId = catMayTinh.CategoryId, AppUserId = adminId },
                new Forum { Name = "Small Form Factor PC", Description = "Máy tính nhỏ gọn ITX", CategoryId = catMayTinh.CategoryId, AppUserId = adminId }
            });

            // --- NHÓM 2: SẢN PHẨM CÔNG NGHỆ ---
            var catCongNghe = new Category { Name = "Sản phẩm công nghệ", Description = "Các thiết bị di động và gia dụng" };
            context.Categories.Add(catCongNghe);
            await context.SaveChangesAsync();

            context.Forums.AddRange(new List<Forum>
            {
                new Forum { Name = "Android", Description = "Google Android & điện thoại Android", CategoryId = catCongNghe.CategoryId, AppUserId = adminId },
                new Forum { Name = "Apple", Description = "iOS, macOS, iPhone, iPad, MacBook", CategoryId = catCongNghe.CategoryId, AppUserId = adminId },
                new Forum { Name = "Multimedia", Description = "Âm thanh, hình ảnh, loa đài", CategoryId = catCongNghe.CategoryId, AppUserId = adminId },
                new Forum { Name = "Đồ điện tử & Thiết bị gia dụng", Description = "Smart home, Tivi, Tủ lạnh...", CategoryId = catCongNghe.CategoryId, AppUserId = adminId },
                new Forum { Name = "Chụp ảnh & Quay phim", Description = "Máy ảnh, ống kính, kỹ thuật chụp", CategoryId = catCongNghe.CategoryId, AppUserId = adminId },
                new Forum { Name = "Góc chiến lược", Description = "Show góc làm việc, giải trí", CategoryId = catCongNghe.CategoryId, AppUserId = adminId }
            });

            // --- NHÓM 3: HỌC TẬP & SỰ NGHIỆP ---
            var catHocTap = new Category { Name = "Học tập & Sự nghiệp", Description = "Định hướng tương lai" };
            context.Categories.Add(catHocTap);
            await context.SaveChangesAsync();

            context.Forums.AddRange(new List<Forum>
            {
                new Forum { Name = "Tuyển dụng - Tìm việc", Description = "Thông tin việc làm, tuyển dụng", CategoryId = catHocTap.CategoryId, AppUserId = adminId },
                new Forum { Name = "Ngoại ngữ", Description = "Tiếng Anh, Nhật, Hàn, Trung...", CategoryId = catHocTap.CategoryId, AppUserId = adminId },
                new Forum { Name = "Lập trình / CNTT", Description = "Thảo luận về Coding, Software", CategoryId = catHocTap.CategoryId, AppUserId = adminId },
                new Forum { Name = "Kinh tế / Luật", Description = "Kiến thức kinh tế, luật pháp", CategoryId = catHocTap.CategoryId, AppUserId = adminId },
                new Forum { Name = "Make Money Online", Description = "Kiếm tiền trên mạng (MMO)", CategoryId = catHocTap.CategoryId, AppUserId = adminId },
                 new Forum { Name = "Tiền điện tử", Description = "Crypto, Blockchain", CategoryId = catHocTap.CategoryId, AppUserId = adminId }
            });

            // --- NHÓM 4: KHU VUI CHƠI GIẢI TRÍ ---
            var catGiaiTri = new Category { Name = "Khu vui chơi giải trí", Description = "Thư giãn, chém gió" };
            context.Categories.Add(catGiaiTri);
            await context.SaveChangesAsync();

            context.Forums.AddRange(new List<Forum>
            {
                new Forum { Name = "Chuyện trò linh tinh", Description = "F17 - Nơi huyền thoại bắt đầu", CategoryId = catGiaiTri.CategoryId, AppUserId = adminId },
                new Forum { Name = "Điểm báo", Description = "F33 - Tin tức thời sự", CategoryId = catGiaiTri.CategoryId, AppUserId = adminId },
                new Forum { Name = "Thể dục - Thể thao", Description = "Bóng đá, Gym, Running", CategoryId = catGiaiTri.CategoryId, AppUserId = adminId },
                new Forum { Name = "Phim / Nhạc / Sách", Description = "Văn hóa nghệ thuật", CategoryId = catGiaiTri.CategoryId, AppUserId = adminId },
                new Forum { Name = "Ẩm thực & Du lịch", Description = "Review ăn uống, đi chơi", CategoryId = catGiaiTri.CategoryId, AppUserId = adminId }
            });

            // Lưu tất cả thay đổi
            await context.SaveChangesAsync();
        }

        private static bool IsValidPasswordHash(string? passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash)) return false;

            try
            {
                Convert.FromBase64String(passwordHash);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}