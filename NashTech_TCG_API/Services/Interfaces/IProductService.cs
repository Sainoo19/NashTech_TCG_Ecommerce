using NashTech_TCG_API.Models.DTOs;

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
            bool ascending = true);
        Task<ProductDTO> CreateProductAsync(CreateProductDTO productDTO);
        Task<ProductDTO> UpdateProductAsync(string id, UpdateProductDTO productDTO);
        Task<bool> DeleteProductAsync(string id);
        
    }
}
