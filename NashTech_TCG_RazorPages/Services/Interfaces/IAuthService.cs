using NashTech_TCG_ShareViewModels.ViewModels;
using System.Security.Claims;

namespace NashTech_TCG_RazorPages.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, string Token)> LoginAsync(LoginViewModel model);
        Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model);
        Task<ClaimsPrincipal> ValidateTokenAsync(string token);
        Task<List<string>> GetUserRolesAsync(string token);
        Task<UserProfileViewModel> GetUserProfileAsync(string token);
    }
}
