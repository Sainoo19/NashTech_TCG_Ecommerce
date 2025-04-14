using Microsoft.AspNetCore.Identity;
using NashTech_TCG_API.Models;

namespace NashTech_TCG_API.Data
{
    public class DataSeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DataSeeder(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            // Create roles
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Customer"));
            }

            // Create admin user
            var adminUser = await _userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User"
                };

                await _userManager.CreateAsync(adminUser, "Admin@123");
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Create customer user
            var customerUser = await _userManager.FindByEmailAsync("customer@example.com");
            if (customerUser == null)
            {
                customerUser = new ApplicationUser
                {
                    UserName = "customer@example.com",
                    Email = "customer@example.com",
                    EmailConfirmed = true,
                    FirstName = "Customer",
                    LastName = "User"
                };

                await _userManager.CreateAsync(customerUser, "Customer@123");
                await _userManager.AddToRoleAsync(customerUser, "Customer");
            }
        }
    }
}
