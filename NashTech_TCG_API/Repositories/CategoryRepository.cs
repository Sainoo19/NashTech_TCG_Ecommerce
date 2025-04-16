using Microsoft.EntityFrameworkCore;
using NashTech_TCG_API.Data;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;

namespace NashTech_TCG_API.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(IEnumerable<Category> Categories, int TotalCount)> GetPagedCategoriesAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true)
        {
            var query = _dbContext.Categories.AsQueryable();

            // Apply search if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.Name.Contains(searchTerm) ||
                    (c.Description != null && c.Description.Contains(searchTerm)));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = ascending
                            ? query.OrderBy(c => c.Name)
                            : query.OrderByDescending(c => c.Name);
                        break;
                    case "id":
                        query = ascending
                            ? query.OrderBy(c => c.CategoryId)
                            : query.OrderByDescending(c => c.CategoryId);
                        break;
                    default:
                        query = ascending
                            ? query.OrderBy(c => c.Name)
                            : query.OrderByDescending(c => c.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(c => c.Name);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var categories = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (categories, totalCount);
        }
    }
}
