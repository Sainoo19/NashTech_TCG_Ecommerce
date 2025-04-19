using NashTech_TCG_API.Models;
using static NashTech_TCG_API.Models.DTOs.ProductVariantDTOs;

namespace NashTech_TCG_API.Repositories.Interfaces
{
    public interface IProductVariantRepository : IRepository<ProductVariant>
    {
        Task<(IEnumerable<ProductVariant> ProductVariants, int TotalCount)> GetPagedProductVariantsAsync(
            int pageNumber,
            int pageSize,
            string productId = null,
            string rarityId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true);

        Task<ProductVariant> GetByProductAndRarityAsync(string productId, string rarityId);
    }
}