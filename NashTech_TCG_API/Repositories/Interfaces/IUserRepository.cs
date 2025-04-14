using NashTech_TCG_API.Models;
using System.Security.Claims;

namespace NashTech_TCG_API.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        Task<ApplicationUser> GetByEmailAsync(string email);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<bool> CreateUserAsync(ApplicationUser user, string password);
        Task AddToRoleAsync(ApplicationUser user, string role);
        Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(ApplicationUser user);
        Task<IList<string>> GetUserRolesByIdAsync(string userId);

    }
}
