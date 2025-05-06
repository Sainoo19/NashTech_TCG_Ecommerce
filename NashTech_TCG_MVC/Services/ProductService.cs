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
    public class ProductService : BaseHttpService, IProductService
    {
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProductService> logger)
            : base(httpClientFactory, httpContextAccessor)
        {
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

            return await GetAsync<PagedProductViewModel>(endpoint, _logger);
        }

        public async Task<(bool Success, string Message, ProductViewModel Data)> GetProductByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return (false, "Product ID cannot be empty", null);
            }

            return await GetAsync<ProductViewModel>($"api/product/{id}", _logger);
        }

        public async Task<(bool Success, string Message, IEnumerable<CategoryViewModel> Data)> GetAllCategoriesAsync()
        {
            var result = await GetAsync<PagedResultViewModel<CategoryViewModel>>("api/Category", _logger);
            return result.Success
                ? (true, result.Message, result.Data.Items)
                : (false, result.Message, null);
        }

        public async Task<(bool Success, string Message, ProductViewModel Data)> GetProductDetailsAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return (false, "Product ID cannot be empty", null);
            }

            return await GetAsync<ProductViewModel>($"api/product/details/{id}", _logger);
        }

        public async Task<(bool Success, string Message, ProductRatingViewModel Data)> AddProductRatingAsync(ProductRatingInputViewModel model)
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(token))
            {
                return (false, "You must be logged in to submit a rating", null);
            }

            return await PostAsync<ProductRatingViewModel>("api/product/rating", model, _logger);
        }

        public async Task<(bool Success, string Message, IEnumerable<ProductViewModel> Data)> GetBestSellingProductsAsync(int limit = 8)
        {
            // Set a reasonable limit
            if (limit <= 0 || limit > 20)
                limit = 8;

            return await GetAsync<IEnumerable<ProductViewModel>>($"api/product/best-selling?limit={limit}", _logger);
        }

        public async Task<(bool Success, string Message, IEnumerable<ProductViewModel> Data)> GetRelatedProductsAsync(string productId, int limit = 5)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return (false, "Product ID cannot be empty", null);
            }

            // Set a reasonable limit
            if (limit <= 0 || limit > 20)
                limit = 5;

            return await GetAsync<IEnumerable<ProductViewModel>>($"api/product/related/{productId}?limit={limit}", _logger);
        }
    }
}