using Microsoft.AspNetCore.Identity;
using Books.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace Books.Models
{
    public class SeedDataPlease
    {
        public static async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var RoleManager1 = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<BooksUser>>();
            var UserManager1 = serviceProvider.GetRequiredService<UserManager<BooksUser>>();
            IdentityResult roleResult;
            IdentityResult identityResult;
            //Add Admin Role
            var roleCheck = await RoleManager.RoleExistsAsync("Admin");
            if (!roleCheck) { roleResult = await RoleManager.CreateAsync(new IdentityRole("Admin")); }

            var roleCheck1 = await RoleManager1.RoleExistsAsync("User");
            if (!roleCheck1) { identityResult = await RoleManager1.CreateAsync(new IdentityRole("User")); }

            BooksUser user = await UserManager.FindByEmailAsync("admin@grmail.com");
            if (user == null)
            {
                var User = new BooksUser();
                User.Email = "admin@grmail.com";
                User.UserName = "admin@grmail.com";
                string userPWD = "Admin123";
                IdentityResult chkUser = await UserManager.CreateAsync(User, userPWD);
                //Add default User to Role Admin 
                if (chkUser.Succeeded) { var result = await UserManager.AddToRoleAsync(User, "Admin"); }
            }

            BooksUser user1 = await UserManager1.FindByEmailAsync("user@grmail.com");
            if (user1 == null)
            {
                var User = new BooksUser();
                User.Email = "user@grmail.com";
                User.UserName = "user@grmail.com";
                string userPWD = "User123";
                IdentityResult chkUser = await UserManager1.CreateAsync(User, userPWD);
                //Add default User to Role Admin 
                if (chkUser.Succeeded) { var result1 = await UserManager1.AddToRoleAsync(User, "User"); }
            }
        }

        public static async Task CreateUserRoles1(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var RoleManager1 = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<BooksUser>>();
            var UserManager1 = serviceProvider.GetRequiredService<UserManager<BooksUser>>();
            IdentityResult roleResult;
            IdentityResult identityResult;
            //Add Admin Role
            var roleCheck = await RoleManager.RoleExistsAsync("Admin");
            if (!roleCheck) { roleResult = await RoleManager.CreateAsync(new IdentityRole("Admin")); }

            var roleCheck1 = await RoleManager1.RoleExistsAsync("User");
            if (!roleCheck1) { identityResult = await RoleManager1.CreateAsync(new IdentityRole("User")); }

            BooksUser user = await UserManager.FindByEmailAsync("admin@grmail.com");
            if (user == null)
            {
                var User = new BooksUser();
                User.Email = "admin@grmail.com";
                User.UserName = "admin@grmail.com";
                string userPWD = "Admin123";
                IdentityResult chkUser = await UserManager.CreateAsync(User, userPWD);
                //Add default User to Role Admin 
                if (chkUser.Succeeded) { var result = await UserManager.AddToRoleAsync(User, "Admin"); }
            }

            BooksUser user1 = await UserManager1.FindByEmailAsync("user@grmail.com");
            if (user1 == null)
            {
                var User = new BooksUser();
                User.Email = "user@grmail.com";
                User.UserName = "user@grmail.com";
                string userPWD = "User123";
                IdentityResult chkUser = await UserManager1.CreateAsync(User, userPWD);
                //Add default User to Role Admin 
                if (chkUser.Succeeded) { var result1 = await UserManager1.AddToRoleAsync(User, "User"); }
            }
        }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new Books.Data.BooksContext(
            serviceProvider.GetRequiredService<
            DbContextOptions<Books.Data.BooksContext>>()))
            {
                CreateUserRoles(serviceProvider).Wait();
                CreateUserRoles1(serviceProvider).Wait();
            }
        }
    }
}
