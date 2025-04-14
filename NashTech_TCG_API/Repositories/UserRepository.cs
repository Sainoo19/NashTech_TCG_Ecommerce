using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NashTech_TCG_API.Data;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;
using OpenIddict.Abstractions;

namespace NashTech_TCG_API.Repositories
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserRepository(
            AppDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) : base(dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<ApplicationUser> GetByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
            return result.Succeeded;
        }

        public async Task<bool> CreateUserAsync(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result.Succeeded;
        }

        public async Task AddToRoleAsync(ApplicationUser user, string role)
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(ApplicationUser user)
        {
            // Create a new ClaimsIdentity
            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                Claims.Name,
                Claims.Role);

            identity.AddClaim(new Claim(Claims.Subject, user.Id));
            identity.AddClaim(new Claim(Claims.Name, user.UserName));
            //identity.AddClaim(new Claim(Claims.Email, user.Email));
            if (!string.IsNullOrEmpty(user.Email))
            {
                identity.AddClaim(new Claim(Claims.Email, user.Email)
                    .SetDestinations(OpenIddictConstants.Destinations.AccessToken,
                                    OpenIddictConstants.Destinations.IdentityToken));
            }

            // Add role claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(Claims.Role, role));
            }

            // Set custom claim for redirect URI based on role
            if (roles.Contains("Admin"))
            {
                identity.AddClaim(new Claim("redirect_uri", "https://localhost:5002"));
            }
            else
            {
                identity.AddClaim(new Claim("redirect_uri", "https://localhost:5001"));
            }

            var principal = new ClaimsPrincipal(identity);

            // ⚠️ Bắt buộc phải set scopes ở đây
            //principal.SetScopes("api", "roles", "offline_access");
            principal.SetScopes("api", "roles", "email", "offline_access");


            return principal;
        }

        public async Task<IList<string>> GetUserRolesByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new List<string>();

            return await _userManager.GetRolesAsync(user);
        }

    }
}
