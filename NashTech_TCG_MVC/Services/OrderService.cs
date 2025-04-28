using NashTech_TCG_MVC.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;
using System.Text.Json;
using System.Text;

namespace NashTech_TCG_MVC.Services
{
    public class OrderService : IOrderService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
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
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<(bool Success, string Message, string OrderId)> PlaceOrderAsync(PlaceOrderViewModel model)
        {
            try
            {
                var client = await CreateClientWithAuthAsync();
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Log request body for debugging
                Console.WriteLine($"Request body: {json}");

                var response = await client.PostAsync("api/Order", content);

                // Get complete response content for debugging
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response status: {response.StatusCode}, Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    return (false, $"Failed to place order: {responseContent}", null);
                }

                var result = JsonSerializer.Deserialize<Models.Responses.ApiResponse<OrderViewModel>>(
                    responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Data == null)
                {
                    return (false, "Invalid response from server", null);
                }

                return (true, result.Message ?? "Order placed successfully", result.Data.OrderId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in PlaceOrderAsync: {ex.Message}");
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }


        public async Task<(bool Success, string Message, OrderViewModel Data)> GetOrderByIdAsync(string orderId)
        {
            try
            {
                var client = await CreateClientWithAuthAsync();
                var response = await client.GetAsync($"api/Order/{orderId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Failed to retrieve order: {errorContent}", null);
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Models.Responses.ApiResponse<OrderViewModel>>(
                    content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return (true, result?.Message ?? "Order retrieved successfully", result?.Data);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }
    }
}
