using NashTech_TCG_API.Models;

namespace NashTech_TCG_API.Repositories.Interfaces
{
    public interface ICustomerRepository : IRepository<ApplicationUser>
    {
        Task<(IEnumerable<ApplicationUser> Customers, int TotalCount)> GetPagedCustomersAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true);

        Task<bool> IsInRoleAsync(ApplicationUser user, string role);
        Task<bool> UpdateCustomerAsync(ApplicationUser user);
    }
}
