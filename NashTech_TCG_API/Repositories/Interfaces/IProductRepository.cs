using NashTech_TCG_API.Models;

namespace NashTech_TCG_API.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            string categoryId = null,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true);
        
        Task<IEnumerable<Product>> GetTopRatedProductsByCategoryAsync(string categoryId,int limit = 8);

    }
}