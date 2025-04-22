using NashTech_TCG_ShareViewModels.ViewModels;
using System.Net.Http.Headers;
using NashTech_TCG_MVC.Services.Interfaces;
using NashTech_TCG_MVC.Models.Responses;
using System.Text.Json;
using System.Text;

namespace NashTech_TCG_MVC.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<HttpClient> CreateClientWithAuthAsync()
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

        public async Task<(bool Success, string Message, CartViewModel Data)> GetCartAsync()
        {
            try
            {
                var client = await CreateClientWithAuthAsync();
                var response = await client.GetAsync("api/Cart");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Failed to retrieve cart: {errorContent}", null);
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<CartViewModel>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return (true, result?.Message ?? "Cart retrieved successfully", result?.Data);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, CartItemViewModel Data)> AddToCartAsync(AddToCartViewModel model)
        {
            try
            {
                var client = await CreateClientWithAuthAsync();
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Cart/items", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Failed to add item to cart: {errorContent}", null);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<CartItemViewModel>>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return (true, result?.Message ?? "Item added to cart successfully", result?.Data);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, CartItemViewModel Data)> UpdateCartItemAsync(UpdateCartItemViewModel model)
        {
            try
            {
                var client = await CreateClientWithAuthAsync();
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync("api/Cart/items", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Failed to update cart item: {errorContent}", null);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<CartItemViewModel>>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return (true, result?.Message ?? "Cart item updated successfully", result?.Data);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> RemoveCartItemAsync(string cartItemId)
        {
            try
            {
                var client = await CreateClientWithAuthAsync();
                var response = await client.DeleteAsync($"api/Cart/items/{cartItemId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Failed to remove cart item: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<object>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return (true, result?.Message ?? "Cart item removed successfully");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ClearCartAsync()
        {
            try
            {
                var client = await CreateClientWithAuthAsync();
                var response = await client.DeleteAsync("api/Cart");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Failed to clear cart: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<object>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return (true, result?.Message ?? "Cart cleared successfully");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }
    }
}
