using static NashTech_TCG_API.Models.DTOs.ProductVariantDTOs;

namespace NashTech_TCG_API.Services.Interfaces
{
    public interface IProductVariantService
    {
        Task<ProductVariantDTO> GetByIdAsync(string id);
        Task<IEnumerable<ProductVariantDTO>> GetAllAsync();
        Task<(IEnumerable<ProductVariantDTO> ProductVariants, int TotalCount, int TotalPages)> GetPagedProductVariantsAsync(
            int pageNumber,
            int pageSize,
            string productId = null,
            string rarityId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true);
        Task<ProductVariantDTO> CreateProductVariantAsync(CreateProductVariantDTO productVariantDTO);
        Task<ProductVariantDTO> UpdateProductVariantAsync(string id, UpdateProductVariantDTO productVariantDTO);
        Task<bool> DeleteProductVariantAsync(string id);
    }
}
