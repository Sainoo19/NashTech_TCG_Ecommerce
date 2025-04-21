using NashTech_TCG_API.Models;

namespace NashTech_TCG_API.Repositories.Interfaces
{
    public interface IRarityRepository : IRepository<Rarity>
    {

        Task<(IEnumerable<Rarity> Rarities, int TotalCount)> GetPagedRaritiesAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true);
        Task<Rarity> GetByIdAsync(string id);
    }
}
