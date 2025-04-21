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


        public async Task<IEnumerable<Product>> GetTopRatedProductsByCategoryAsync(string categoryId, int limit = 8)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(categoryId))
                {
                    throw new ArgumentException("Category ID is required", nameof(categoryId));
                }

                // Get products for this specific category with their ratings
                var products = await _dbContext.Products
                    .AsNoTracking()
                    .Where(p => p.CategoryId == categoryId)
                    .Include(p => p.Category)
                    .Include(p => p.ProductRatings)
                    .ToListAsync();

                // Calculate average rating for each product and sort
                var productsWithRating = products.Select(p => new
                {
                    Product = p,
                    AverageRating = p.ProductRatings.Any() ? p.ProductRatings.Average(r => r.Rating) : 0,
                    RatingCount = p.ProductRatings.Count
                })
                // First sort by average rating (descending)
                .OrderByDescending(p => p.AverageRating)
                // Then by number of ratings (descending) as tiebreaker for products with same rating
                .ThenByDescending(p => p.RatingCount)
                // Limit to requested number
                .Take(limit)
                .Select(p => p.Product)
                .ToList();

                return productsWithRating;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving top rated products for category {categoryId}", ex);
            }
        }



    }
}
