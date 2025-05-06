using NashTech_TCG_ShareViewModels.ViewModels;
using System.Net.Http.Headers;
using NashTech_TCG_MVC.Services.Interfaces;
using NashTech_TCG_MVC.Models.Responses;
using System.Text.Json;
using System.Text;

namespace NashTech_TCG_MVC.Services
{
    public class CartService : BaseHttpService, ICartService
    {
        private readonly ILogger<CartService> _logger;

        public CartService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CartService> logger = null)
            : base(httpClientFactory, httpContextAccessor)
        {
            _logger = logger;
        }

        public async Task<(bool Success, string Message, CartViewModel Data)> GetCartAsync()
        {
            return await GetAsync<CartViewModel>("api/Cart", _logger);
        }

        public async Task<(bool Success, string Message, CartItemViewModel Data)> AddToCartAsync(AddToCartViewModel model)
        {
            return await PostAsync<CartItemViewModel>("api/Cart/items", model, _logger);
        }

        public async Task<(bool Success, string Message, CartItemViewModel Data)> UpdateCartItemAsync(UpdateCartItemViewModel model)
        {
            return await PutAsync<CartItemViewModel>("api/Cart/items", model, _logger);
        }

        public async Task<(bool Success, string Message)> RemoveCartItemAsync(string cartItemId)
        {
            return await DeleteAsync($"api/Cart/items/{cartItemId}", _logger);
        }

        public async Task<(bool Success, string Message)> ClearCartAsync()
        {
            return await DeleteAsync("api/Cart", _logger);
        }
    }
}
