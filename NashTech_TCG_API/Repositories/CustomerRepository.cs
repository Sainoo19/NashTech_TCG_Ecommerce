using Microsoft.AspNetCore.Identity;
using NashTech_TCG_API.Data;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;

namespace NashTech_TCG_API.Repositories
{
    public class CustomerRepository : Repository<ApplicationUser>, ICustomerRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerRepository(
            AppDbContext dbContext,
            UserManager<ApplicationUser> userManager) : base(dbContext)
        {
            _userManager = userManager;
        }

        public async Task<(IEnumerable<ApplicationUser> Customers, int TotalCount)> GetPagedCustomersAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true)
        {
            // Get users who are in the customer role
            var customerRoleUsers = await _userManager.GetUsersInRoleAsync("Customer");
            var query = customerRoleUsers.AsQueryable();

            // Apply search if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.UserName.ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "username":
                        query = ascending
                            ? query.OrderBy(u => u.UserName)
                            : query.OrderByDescending(u => u.UserName);
                        break;
                    case "email":
                        query = ascending
                            ? query.OrderBy(u => u.Email)
                            : query.OrderByDescending(u => u.Email);
                        break;
                    case "firstname":
                        query = ascending
                            ? query.OrderBy(u => u.FirstName)
                            : query.OrderByDescending(u => u.FirstName);
                        break;
                    case "lastname":
                        query = ascending
                            ? query.OrderBy(u => u.LastName)
                            : query.OrderByDescending(u => u.LastName);
                        break;
                    default:
                        query = ascending
                            ? query.OrderBy(u => u.UserName)
                            : query.OrderByDescending(u => u.UserName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(u => u.UserName);
            }

            // Get total count for pagination
            var totalCount = query.Count();

            // Apply pagination
            var customers = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (customers, totalCount);
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string role)
        {
            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<bool> UpdateCustomerAsync(ApplicationUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
