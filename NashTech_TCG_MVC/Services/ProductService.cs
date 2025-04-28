using NashTech_TCG_MVC.Models.Responses;
using NashTech_TCG_MVC.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace NashTech_TCG_MVC.Services
{
    public class ProductService : IProductService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProductService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, PagedProductViewModel Data)> GetPagedProductsAsync(
            string categoryId = null,
            string searchTerm = null,
            string sortBy = "name",
            bool ascending = true,
            int pageNumber = 1,
            int pageSize = 9)
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

                // Build query string
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(categoryId))
                    queryParams.Add($"categoryId={categoryId}");
                if (!string.IsNullOrEmpty(searchTerm))
                    queryParams.Add($"searchTerm={searchTerm}");
                queryParams.Add($"sortBy={sortBy}");
                queryParams.Add($"ascending={ascending}");
                queryParams.Add($"pageNumber={pageNumber}");
                queryParams.Add($"pageSize={pageSize}");

                var queryString = string.Join("&", queryParams);
                var endpoint = $"api/product/client?{queryString}";

                _logger.LogInformation($"Requesting products from API: {endpoint}");
                var response = await client.GetAsync(endpoint);

                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedProductViewModel>>(content, options);

                    if (apiResponse.Success)
                    {
                        _logger.LogInformation($"Successfully retrieved {apiResponse.Data?.Products?.Count() ?? 0} products");
                        return (true, "Products retrieved successfully", apiResponse.Data);
                    }

                    _logger.LogWarning($"API returned error: {apiResponse.Message}");
                    return (false, apiResponse.Message, null);
                }

                _logger.LogWarning($"API request failed with status {response.StatusCode}: {content}");
                return (false, $"Failed to retrieve products: {response.StatusCode}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products from API");
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, ProductViewModel Data)> GetProductByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return (false, "Product ID cannot be empty", null);
                }

                var client = _httpClientFactory.CreateClient("API");

                // Add authorization token if available
                var token = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                _logger.LogInformation($"Requesting product details for ID: {id}");
                var response = await client.GetAsync($"api/product/{id}");

                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductViewModel>>(content, options);

                    if (apiResponse.Success)
                    {
                        _logger.LogInformation($"Successfully retrieved product: {apiResponse.Data?.Name}");
                        return (true, "Product retrieved successfully", apiResponse.Data);
                    }

                    _logger.LogWarning($"API returned error: {apiResponse.Message}");
                    return (false, apiResponse.Message, null);
                }

                _logger.LogWarning($"API request failed with status {response.StatusCode}: {content}");
                return (false, $"Failed to retrieve product: {response.StatusCode}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product {id} from API");
                return (false, $"An error occurred: {ex.Message}", null);
            }
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

        // Add to MVC ProductService.cs
        public async Task<(bool Success, string Message, ProductViewModel Data)> GetProductDetailsAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return (false, "Product ID cannot be empty", null);
                }

                var client = _httpClientFactory.CreateClient("API");

                // Add authorization token if available
                var token = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                _logger.LogInformation($"Requesting product details for ID: {id}");
                var response = await client.GetAsync($"api/product/details/{id}");

                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductViewModel>>(content, options);

                    if (apiResponse.Success)
                    {
                        _logger.LogInformation($"Successfully retrieved product details: {apiResponse.Data?.Name}");
                        return (true, "Product details retrieved successfully", apiResponse.Data);
                    }

                    _logger.LogWarning($"API returned error: {apiResponse.Message}");
                    return (false, apiResponse.Message, null);
                }

                _logger.LogWarning($"API request failed with status {response.StatusCode}: {content}");
                return (false, $"Failed to retrieve product details: {response.StatusCode}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product details {id} from API");
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, ProductRatingViewModel Data)> AddProductRatingAsync(ProductRatingInputViewModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("API");

                // Add authorization token if available
                var token = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    return (false, "You must be logged in to submit a rating", null);
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Serialize the model to JSON
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Send the POST request
                _logger.LogInformation($"Submitting rating for product {model.ProductId}");
                var response = await client.PostAsync("api/product/rating", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductRatingViewModel>>(responseContent, options);

                    if (apiResponse.Success)
                    {
                        _logger.LogInformation($"Successfully submitted rating for product {model.ProductId}");
                        return (true, "Rating submitted successfully", apiResponse.Data);
                    }

                    _logger.LogWarning($"API returned error: {apiResponse.Message}");
                    return (false, apiResponse.Message, null);
                }

                _logger.LogWarning($"API request failed with status {response.StatusCode}: {responseContent}");
                return (false, $"Failed to submit rating: {response.StatusCode}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting rating for product {model.ProductId}");
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, IEnumerable<ProductViewModel> Data)> GetBestSellingProductsAsync(int limit = 8)
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

                // Set a reasonable limit
                if (limit <= 0 || limit > 20)
                    limit = 8;

                _logger.LogInformation($"Requesting best selling products from API with limit: {limit}");
                var response = await client.GetAsync($"api/product/best-selling?limit={limit}");

                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<ProductViewModel>>>(content, options);

                    if (apiResponse.Success)
                    {
                        _logger.LogInformation($"Successfully retrieved {apiResponse.Data?.Count() ?? 0} best selling products");
                        return (true, "Best selling products retrieved successfully", apiResponse.Data);
                    }

                    _logger.LogWarning($"API returned error: {apiResponse.Message}");
                    return (false, apiResponse.Message, null);
                }

                _logger.LogWarning($"API request failed with status {response.StatusCode}: {content}");
                return (false, $"Failed to retrieve best selling products: {response.StatusCode}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving best selling products from API");
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        
        public async Task<(bool Success, string Message, IEnumerable<ProductViewModel> Data)> GetRelatedProductsAsync(string productId, int limit = 5)
        {
            try
            {
                if (string.IsNullOrEmpty(productId))
                {
                    return (false, "Product ID cannot be empty", null);
                }

                var client = _httpClientFactory.CreateClient("API");

                // Add authorization token if available
                var token = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // Set a reasonable limit
                if (limit <= 0 || limit > 20)
                    limit = 5;

                _logger.LogInformation($"Requesting related products for product ID: {productId} with limit: {limit}");
                var response = await client.GetAsync($"api/product/related/{productId}?limit={limit}");

                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<ProductViewModel>>>(content, options);

                    if (apiResponse.Success)
                    {
                        _logger.LogInformation($"Successfully retrieved {apiResponse.Data?.Count() ?? 0} related products");
                        return (true, "Related products retrieved successfully", apiResponse.Data);
                    }

                    _logger.LogWarning($"API returned error: {apiResponse.Message}");
                    return (false, apiResponse.Message, null);
                }

                _logger.LogWarning($"API request failed with status {response.StatusCode}: {content}");
                return (false, $"Failed to retrieve related products: {response.StatusCode}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving related products for product {productId} from API");
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }




    }
}
