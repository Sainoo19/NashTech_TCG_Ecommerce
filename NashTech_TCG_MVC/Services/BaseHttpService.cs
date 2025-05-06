using NashTech_TCG_MVC.Models.Responses;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NashTech_TCG_MVC.Services
{
    public class BaseHttpService
    {
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly JsonSerializerOptions _jsonOptions;

        protected BaseHttpService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        protected async Task<HttpClient> CreateClientWithAuthAsync()
        {
            var client = _httpClientFactory.CreateClient("API");

            // Add authorization token if available
            var token = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        protected async Task<(bool Success, string Message, T Data)> GetAsync<T>(string endpoint, ILogger logger = null)
        {
            try
            {
                var client = await CreateClientWithAuthAsync();

                logger?.LogInformation($"Requesting data from API: {endpoint}");
                var response = await client.GetAsync(endpoint);

                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);

                    if (apiResponse.Success)
                    {
                        logger?.LogInformation($"Successfully retrieved data from {endpoint}");
                        return (true, apiResponse.Message ?? "Data retrieved successfully", apiResponse.Data);
                    }

                    logger?.LogWarning($"API returned error: {apiResponse.Message}");
                    return (false, apiResponse.Message, default);
                }

                logger?.LogWarning($"API request failed with status {response.StatusCode}: {content}");
                return (false, $"Failed to retrieve data: {response.StatusCode}", default);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Error accessing API endpoint {endpoint}");
                return (false, $"An error occurred: {ex.Message}", default);
            }
        }

        protected async Task<(bool Success, string Message, T Data)> PostAsync<T>(string endpoint, object data, ILogger logger = null)
        {
            try
            {
                var client = await CreateClientWithAuthAsync();
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                logger?.LogInformation($"Sending POST request to API: {endpoint}");
                var response = await client.PostAsync(endpoint, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);

                    if (apiResponse.Success)
                    {
                        logger?.LogInformation($"Successfully posted data to {endpoint}");
                        return (true, apiResponse.Message ?? "Operation successful", apiResponse.Data);
                    }

                    logger?.LogWarning($"API returned error: {apiResponse.Message}");
                    return (false, apiResponse.Message, default);
                }

                logger?.LogWarning($"API request failed with status {response.StatusCode}: {responseContent}");
                return (false, $"Operation failed: {response.StatusCode}", default);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Error accessing API endpoint {endpoint}");
                return (false, $"An error occurred: {ex.Message}", default);
            }
        }

        protected async Task<(bool Success, string Message, T Data)> PutAsync<T>(string endpoint, object data, ILogger logger = null)
        {
            try
            {
                var client = await CreateClientWithAuthAsync();
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                logger?.LogInformation($"Sending PUT request to API: {endpoint}");
                var response = await client.PutAsync(endpoint, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);

                    if (apiResponse.Success)
                    {
                        logger?.LogInformation($"Successfully updated data at {endpoint}");
                        return (true, apiResponse.Message ?? "Update successful", apiResponse.Data);
                    }

                    logger?.LogWarning($"API returned error: {apiResponse.Message}");
                    return (false, apiResponse.Message, default);
                }

                logger?.LogWarning($"API request failed with status {response.StatusCode}: {responseContent}");
                return (false, $"Update failed: {response.StatusCode}", default);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Error accessing API endpoint {endpoint}");
                return (false, $"An error occurred: {ex.Message}", default);
            }
        }

        protected async Task<(bool Success, string Message)> DeleteAsync(string endpoint, ILogger logger = null)
        {
            try
            {
                var client = await CreateClientWithAuthAsync();

                logger?.LogInformation($"Sending DELETE request to API: {endpoint}");
                var response = await client.DeleteAsync(endpoint);

                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);

                    if (apiResponse.Success)
                    {
                        logger?.LogInformation($"Successfully deleted data at {endpoint}");
                        return (true, apiResponse.Message ?? "Delete successful");
                    }

                    logger?.LogWarning($"API returned error: {apiResponse.Message}");
                    return (false, apiResponse.Message);
                }

                logger?.LogWarning($"API request failed with status {response.StatusCode}: {responseContent}");
                return (false, $"Delete failed: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Error accessing API endpoint {endpoint}");
                return (false, $"An error occurred: {ex.Message}");
            }
        }
    }
}