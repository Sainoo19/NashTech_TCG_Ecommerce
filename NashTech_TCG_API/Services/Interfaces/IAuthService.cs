using NashTech_TCG_API.Models;
using System.Security.Claims;
using NashTech_TCG_ShareViewModels.ViewModels;
using NashTech_TCG_API.Models.DTOs;

namespace NashTech_TCG_API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterUserAsync(RegisterViewModel model);
        Task<ApplicationUser> ValidateUserAsync(string username, string password);
        Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(ApplicationUser user);
        Task<IList<string>> GetUserRolesByIdAsync(string userId);
        Task<IList<string>> GetUserRolesAsync(string email);
        Task<UserProfileViewModel> GetUserProfileAsync(string userId);
    }
}
