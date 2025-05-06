using NashTech_TCG_ShareViewModels.ViewModels;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using NashTech_TCG_MVC.Models.Responses;
using NashTech_TCG_MVC.Services.Interfaces;

namespace NashTech_TCG_MVC.Services
{
    public class AuthService : BaseHttpService, IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<AuthService> logger = null)
            : base(httpClientFactory, httpContextAccessor)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, string Token)> LoginAsync(LoginViewModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("API");

                // Prepare form data for OpenIddict token endpoint
                var formData = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", model.Email },
                    { "password", model.Password },
                    { "scope", " offline_access api roles email " },
                };

                // Send POST request to get token
                var content = new FormUrlEncodedContent(formData);
                var response = await client.PostAsync("api/Auth/token", content);

                // Handle response
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Login failed: {errorContent}", null);
                }

                // Parse successful response
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                return (true, "Login successful", tokenResponse?.AccessToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during login");
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("API");

                // Serialize model to JSON
                var json = JsonSerializer.Serialize(model, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Send POST request for registration
                _logger?.LogInformation($"Sending registration request for email: {model.Email}");
                var response = await client.PostAsync("api/Auth/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Parse response
                ApiResponse<object> result;
                try
                {
                    result = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to parse registration response");
                    return (false, $"Failed to parse response: {ex.Message}");
                }

                // Check response status
                if (!response.IsSuccessStatusCode)
                {
                    return (false, result?.Message ?? $"Registration failed with status code {response.StatusCode}");
                }

                return (true, "Registration successful");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during registration");
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public Task<ClaimsPrincipal> ValidateTokenAsync(string token)
        {
            try
            {
                // Parse JWT token to get claims
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var claims = jwtToken.Claims.ToList();

                // Create ClaimsIdentity from retrieved claims
                var identity = new ClaimsIdentity(claims, "jwt");
                return Task.FromResult(new ClaimsPrincipal(identity));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating token");
                return Task.FromResult<ClaimsPrincipal>(null);
            }
        }

        public async Task<List<string>> GetUserRolesAsync(string token)
        {
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/Auth/roles");
            if (!response.IsSuccessStatusCode)
            {
                return new List<string>();
            }

            // Parse response
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<RolesResponse>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Data?.Roles?.ToList() ?? new List<string>();
        }

        public async Task<UserProfileViewModel> GetUserProfileAsync(string token)
        {
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/Auth/profile");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Parse response
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<UserProfileViewModel>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Data;
        }
    }
}
