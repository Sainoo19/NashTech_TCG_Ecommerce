using Microsoft.EntityFrameworkCore;
using NashTech_TCG_API.Data;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;

namespace NashTech_TCG_API.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            string categoryId = null,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true)
        {
            var query = _dbContext.Products.Include(p => p.Category).AsQueryable();

            // Apply category filter if provided
            if (!string.IsNullOrWhiteSpace(categoryId))
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // Apply search if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    (p.Description != null && p.Description.Contains(searchTerm)));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = ascending
                            ? query.OrderBy(p => p.Name)
                            : query.OrderByDescending(p => p.Name);
                        break;
                    case "id":
                        query = ascending
                            ? query.OrderBy(p => p.ProductId)
                            : query.OrderByDescending(p => p.ProductId);
                        break;
                    case "category":
                        query = ascending
                            ? query.OrderBy(p => p.Category.Name)
                            : query.OrderByDescending(p => p.Category.Name);
                        break;
                    case "date":
                        query = ascending
                            ? query.OrderBy(p => p.CreatedDate)
                            : query.OrderByDescending(p => p.CreatedDate);
                        break;
                    default:
                        query = ascending
                            ? query.OrderBy(p => p.Name)
                            : query.OrderByDescending(p => p.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(p => p.CreatedDate);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }
    }
}
