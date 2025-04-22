using NashTech_TCG_MVC.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;
using NashTech_TCG_MVC.Models.Responses;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NashTech_TCG_MVC.Services
{
    public class HomeService : IHomeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<HomeService> _logger;

        public HomeService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<HomeService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, IEnumerable<CategoryViewModel> Data)> GetAllCategoriesAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("API");

                // Add authorization token if available
                var token = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                _logger.LogInformation("Requesting categories from API");
                var response = await client.GetAsync("api/Category");

                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedResultViewModel<CategoryViewModel>>>(content, options);

                    if (apiResponse.Success)
                    {
                        _logger.LogInformation($"Successfully retrieved {apiResponse.Data?.Items?.Count() ?? 0} categories");
                        return (true, "Categories retrieved successfully", apiResponse.Data.Items);
                    }

                    _logger.LogWarning($"API returned error: {apiResponse.Message}");
                    return (false, apiResponse.Message, null);
                }

                _logger.LogWarning($"API request failed with status {response.StatusCode}: {content}");
                return (false, $"Failed to retrieve categories: {response.StatusCode}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories from API");
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, IEnumerable<ProductViewModel> Data)> GetTopRatedProductsByCategoryAsync(string categoryId, int limit = 8)
        {
            try
            {
                if (string.IsNullOrEmpty(categoryId))
                {
                    return (false, "Category ID cannot be empty", null);
                }

                var client = _httpClientFactory.CreateClient("API");

                // Add authorization token if available
                var token = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                _logger.LogInformation($"Requesting top rated products for category ID: {categoryId}");
                var response = await client.GetAsync($"api/product/category/{categoryId}/top-rated?limit={limit}");

                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<ProductViewModel>>>(content, options);

                    if (apiResponse.Success)
                    {
                        _logger.LogInformation($"Successfully retrieved {apiResponse.Data?.Count() ?? 0} top rated products");
                        return (true, "Top rated products retrieved successfully", apiResponse.Data);
                    }

                    _logger.LogWarning($"API returned error: {apiResponse.Message}");
                    return (false, apiResponse.Message, null);
                }

                _logger.LogWarning($"API request failed with status {response.StatusCode}: {content}");
                return (false, $"Failed to retrieve top rated products: {response.StatusCode}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving top rated products for category {categoryId} from API");
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }
    }
}