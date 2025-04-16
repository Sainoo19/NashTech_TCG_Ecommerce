using NashTech_TCG_ShareViewModels.ViewModels;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using NashTech_TCG_MVC.Models.Responses;
using NashTech_TCG_MVC.Services.Interfaces;

namespace NashTech_TCG_MVC.Services
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
                // Tạo HTTP client để gọi API
                var client = _httpClientFactory.CreateClient("API");

                // Chuẩn bị form data cho OpenIddict token endpoint
                var formData = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", model.Email },
                    { "password", model.Password },
                    { "scope", " offline_access api roles email " },
                    
                };

                // Gửi yêu cầu POST để lấy token
                var content = new FormUrlEncodedContent(formData);
                var response = await client.PostAsync("api/Auth/token", content);

                // Xử lý phản hồi
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Login failed: {errorContent}", null);
                }

                // Phân tích phản hồi thành công
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
                // Tạo HTTP client để gọi API
                var client = _httpClientFactory.CreateClient("API");

                // In ra thông tin để gỡ lỗi
                Console.WriteLine($"Sending registration request to: {client.BaseAddress}api/Auth/register");
                Console.WriteLine($"Request data: Email={model.Email}, FirstName={model.FirstName}, LastName={model.LastName}");

                // Serialize model thành JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(model, options);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Gửi yêu cầu POST để đăng ký
                var response = await client.PostAsync("api/Auth/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // In ra phản hồi để gỡ lỗi
                Console.WriteLine($"Registration response: {responseContent}");

                // Phân tích phản hồi
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

                // Kiểm tra trạng thái phản hồi
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
                // Phân tích JWT token để lấy claim
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var claims = jwtToken.Claims.ToList();

                // Tạo ClaimsIdentity từ các claim đã lấy được
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
                // Tạo HTTP client để gọi API
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Gửi yêu cầu GET để lấy vai trò
                var response = await client.GetAsync("api/Auth/roles");
                if (!response.IsSuccessStatusCode)
                {
                    return new List<string>();
                }

                // Phân tích phản hồi
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

        public async Task<UserProfileViewModel> GetUserProfileAsync(string token)
        {
            try
            {
                // Tạo HTTP client để gọi API
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Gửi yêu cầu GET để lấy thông tin người dùng
                var response = await client.GetAsync("api/Auth/profile");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                // Phân tích phản hồi
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<UserProfileViewModel>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user profile: {ex.Message}");
                return null;
            }
        }
    }
}
