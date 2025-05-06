using NashTech_TCG_MVC.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;
using System.Text.Json;
using System.Text;

namespace NashTech_TCG_MVC.Services
{
    public class OrderService : BaseHttpService, IOrderService
    {
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OrderService> logger = null)
            : base(httpClientFactory, httpContextAccessor)
        {
            _logger = logger;
        }

        public async Task<(bool Success, string Message, string OrderId)> PlaceOrderAsync(PlaceOrderViewModel model)
        {
            var result = await PostAsync<OrderViewModel>("api/Order", model, _logger);

            if (!result.Success || result.Data == null)
            {
                return (false, result.Message ?? "Invalid response from server", null);
            }

            return (true, result.Message ?? "Order placed successfully", result.Data.OrderId);
        }

        public async Task<(bool Success, string Message, OrderViewModel Data)> GetOrderByIdAsync(string orderId)
        {
            return await GetAsync<OrderViewModel>($"api/Order/{orderId}", _logger);
        }
    }
}
