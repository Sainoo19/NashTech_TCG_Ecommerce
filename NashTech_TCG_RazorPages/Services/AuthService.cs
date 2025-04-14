using NashTech_TCG_RazorPages.Services.Interfaces;
using NashTech_TCG_RazorPages.Models.Responses;
using NashTech_TCG_ShareViewModels.ViewModels;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace NashTech_TCG_RazorPages.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message, string Token)> LoginAsync(LoginViewModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("API");

                // Prepare the form data for OpenIddict token endpoint
                var formData = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", model.Email },
                    { "password", model.Password },
                    { "scope", " offline_access api roles email " }
                };

                var content = new FormUrlEncodedContent(formData);
                var response = await client.PostAsync("api/Auth/token", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Login failed: {errorContent}", null);
                }

                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                return (true, "Login successful", tokenResponse?.AccessToken);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("API");

                // Enable logging to see the actual request being sent
                Console.WriteLine($"Sending registration request to: {client.BaseAddress}api/Auth/register");
                Console.WriteLine($"Request data: Email={model.Email}, FirstName={model.FirstName}, LastName={model.LastName}");

                // Use JsonContent to explicitly control serialization
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(model, options);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Auth/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Registration response: {responseContent}");

                // Try to deserialize the response
                ApiResponse<object> result;
                try
                {
                    result = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (Exception ex)
                {
                    return (false, $"Failed to parse response: {ex.Message}. Raw response: {responseContent}");
                }

                if (!response.IsSuccessStatusCode)
                {
                    return (false, result?.Message ?? $"Registration failed with status code {response.StatusCode}. Response: {responseContent}");
                }

                return (true, "Registration successful");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }


        public async Task<ClaimsPrincipal> ValidateTokenAsync(string token)
        {
            try
            {
                // For simplicity, we'll parse the JWT token
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var claims = jwtToken.Claims.ToList();

                var identity = new ClaimsIdentity(claims, "jwt");
                return new ClaimsPrincipal(identity);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<string>> GetUserRolesAsync(string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("api/Auth/roles");
                if (!response.IsSuccessStatusCode)
                {
                    return new List<string>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<RolesResponse>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Data?.Roles?.ToList() ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
