using NashTech_TCG_API.Models.DTOs;
using NashTech_TCG_ShareViewModels.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NashTech_TCG_API.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductDTO> GetByIdAsync(string id);
        Task<IEnumerable<ProductDTO>> GetAllAsync();
        Task<(IEnumerable<ProductDTO> Products, int TotalCount, int TotalPages)> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            string categoryId = null,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true,
            decimal? minPrice = null,
            decimal? maxPrice = null);
        Task<ProductDTO> CreateProductAsync(CreateProductDTO productDTO);
        Task<ProductDTO> UpdateProductAsync(string id, UpdateProductDTO productDTO);
        Task<bool> DeleteProductAsync(string id);

        Task<(IEnumerable<ProductViewModel> Products, int TotalCount, int TotalPages)> GetPagedProductsForClientAsync(
            int pageNumber,
            int pageSize,
            string categoryId = null,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true);
        
        Task<ProductViewModel> GetProductDetailsAsync(string id);
        Task<ProductRatingViewModel> AddProductRatingAsync(ProductRatingInputViewModel model, string userId);
        Task<IEnumerable<ProductViewModel>> GetTopRatedProductsByCategoryAsync(string categoryId, int limit = 8);
    }
}
