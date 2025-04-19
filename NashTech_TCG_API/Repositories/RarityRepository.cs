using Microsoft.EntityFrameworkCore;
using NashTech_TCG_API.Data;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;

namespace NashTech_TCG_API.Repositories
{
    public class RarityRepository : Repository<Rarity>, IRarityRepository
    {
        public RarityRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(IEnumerable<Rarity> Rarities, int TotalCount)> GetPagedRaritiesAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
        string sortBy = null,
            bool ascending = true)
        {
            var query = _dbContext.Rarities.AsQueryable();

            // Apply search if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r =>
                    r.Name.Contains(searchTerm) ||
                    (r.Description != null && r.Description.Contains(searchTerm)));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = ascending
                            ? query.OrderBy(r => r.Name)
                            : query.OrderByDescending(r => r.Name);
                        break;
                    case "id":
                        query = ascending
                            ? query.OrderBy(r => r.RarityId)
                            : query.OrderByDescending(r => r.RarityId);
                        break;
                    default:
                        query = ascending
                            ? query.OrderBy(r => r.Name)
                            : query.OrderByDescending(r => r.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(r => r.Name);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var rarities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (rarities, totalCount);
        }
    }
}
    
