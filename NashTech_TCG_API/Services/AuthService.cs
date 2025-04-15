using Microsoft.AspNetCore.Identity;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Models.DTOs;
using NashTech_TCG_API.Repositories.Interfaces;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;
using System.Security.Claims;

namespace NashTech_TCG_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> RegisterUserAsync(RegisterViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                return false;

            var user = await _userRepository.GetByEmailAsync(model.Email);
            if (user != null)
                return false; // User already exists

            user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userRepository.CreateUserAsync(user, model.Password);
            if (result)
            {
                await _userRepository.AddToRoleAsync(user, "Customer"); // Default role for registered users
                return true;
            }

            return false;
        }

        public async Task<ApplicationUser> ValidateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetByEmailAsync(username);
            if (user == null || string.IsNullOrEmpty(user.Email))
                return null;

            var isValid = await _userRepository.CheckPasswordAsync(user, password);
            return isValid ? user : null;
        }


        public async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(ApplicationUser user)
        {
            return await _userRepository.CreateClaimsPrincipalAsync(user);
        }
        public async Task<IList<string>> GetUserRolesByIdAsync(string userId)
        {
            return await _userRepository.GetUserRolesByIdAsync(userId);
        }


        public async Task<IList<string>> GetUserRolesAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return new List<string>();

            return await _userRepository.GetUserRolesAsync(user);
        }

        public async Task<UserProfileViewModel> GetUserProfileAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            return new UserProfileViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = await _userRepository.GetUserRolesAsync(user)
            };
        }

    }
}
