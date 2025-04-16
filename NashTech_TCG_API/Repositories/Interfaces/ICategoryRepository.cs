using NashTech_TCG_API.Models;

namespace NashTech_TCG_API.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<(IEnumerable<Category> Categories, int TotalCount)> GetPagedCategoriesAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true);
    }
}
