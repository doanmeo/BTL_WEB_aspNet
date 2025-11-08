using BlogWebsite.Models;
using Microsoft.AspNetCore.Identity;

namespace BlogWebsite.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // Lấy các dịch vụ cần thiết
            var userManager = service.GetRequiredService<UserManager<AppUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();

            // Tạo các vai trò (Roles)
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Tạo tài khoản Admin mặc định
            var adminEmail = "admin@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdminUser = new AppUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(newAdminUser, "Admin@123");

                if (result.Succeeded)
                {
                    // Gán vai trò "Admin" cho tài khoản mới tạo
                    await userManager.AddToRoleAsync(newAdminUser, "Admin");

                    // Tạo UserProfile cho admin
                    var userProfile = new UserProfile
                    {
                        UserId = newAdminUser.Id,
                        DisplayName = "Administrator"
                    };
                    var dbContext = service.GetRequiredService<ApplicationDbContext>();
                    dbContext.UserProfiles.Add(userProfile);
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
