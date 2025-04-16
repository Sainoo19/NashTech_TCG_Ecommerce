using System.Security.Claims;
using NashTech_TCG_ShareViewModels.ViewModels;

namespace NashTech_TCG_MVC.Services.Interfaces
{
    public interface IAuthService
    {
        // Đăng nhập và nhận token
        Task<(bool Success, string Message, string Token)> LoginAsync(LoginViewModel model);

        // Đăng ký người dùng mới
        Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model);

        // Xác thực token nhận được
        Task<ClaimsPrincipal> ValidateTokenAsync(string token);

        // Lấy danh sách vai trò của người dùng
        Task<List<string>> GetUserRolesAsync(string token);

        // Lấy thông tin profile người dùng
        Task<UserProfileViewModel> GetUserProfileAsync(string token);
    }
}
