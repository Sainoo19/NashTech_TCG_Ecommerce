using NashTech_TCG_MVC.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;
using NashTech_TCG_MVC.Models.Responses;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NashTech_TCG_MVC.Services
{
    public class HomeService : BaseHttpService, IHomeService
    {
        private readonly ILogger<HomeService> _logger;

        public HomeService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<HomeService> logger)
            : base(httpClientFactory, httpContextAccessor)
        {
            _logger = logger;
        }

        public async Task<(bool Success, string Message, IEnumerable<CategoryViewModel> Data)> GetAllCategoriesAsync()
        {
            var result = await GetAsync<PagedResultViewModel<CategoryViewModel>>("api/Category", _logger);
            return result.Success
                ? (true, result.Message, result.Data.Items)
                : (false, result.Message, null);
        }

        public async Task<(bool Success, string Message, IEnumerable<ProductViewModel> Data)> GetTopRatedProductsByCategoryAsync(string categoryId, int limit = 8)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return (false, "Category ID cannot be empty", null);
            }

            return await GetAsync<IEnumerable<ProductViewModel>>(
                $"api/product/category/{categoryId}/top-rated?limit={limit}", _logger);
        }
    }
}