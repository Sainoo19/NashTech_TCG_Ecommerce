using NashTech_TCG_ShareViewModels.ViewModels;

namespace NashTech_TCG_MVC.Services.Interfaces
{
    public interface IProductService
    {
        Task<(bool Success, string Message, PagedProductViewModel Data)> GetPagedProductsAsync(
           string categoryId = null,
           string searchTerm = null,
           string sortBy = "name",
           bool ascending = true,
           int pageNumber = 1,
           int pageSize = 9);

        Task<(bool Success, string Message, ProductViewModel Data)> GetProductByIdAsync(string id);

        Task<(bool Success, string Message, IEnumerable<CategoryViewModel> Data)> GetAllCategoriesAsync();

        // Add to MVC IProductService.cs
        Task<(bool Success, string Message, ProductViewModel Data)> GetProductDetailsAsync(string id);
        Task<(bool Success, string Message, ProductRatingViewModel Data)> AddProductRatingAsync(ProductRatingInputViewModel model);
        Task<(bool Success, string Message, IEnumerable<ProductViewModel> Data)> GetBestSellingProductsAsync(int limit = 8);
        Task<(bool Success, string Message, IEnumerable<ProductViewModel> Data)> GetRelatedProductsAsync(string productId, int limit = 5);

    }
}