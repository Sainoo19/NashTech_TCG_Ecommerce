using Microsoft.EntityFrameworkCore;
using NashTech_TCG_API.Data;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;

namespace NashTech_TCG_API.Repositories
{
    public class ProductVariantRepository : Repository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<ProductVariant> GetByProductAndRarityAsync(string productId, string rarityId)
        {
            return await _dbContext.ProductVariants
                .Include(pv => pv.Product)
                .Include(pv => pv.Rarity)
                .FirstOrDefaultAsync(pv => pv.ProductId == productId && pv.RarityId == rarityId);
        }

        public async Task<(IEnumerable<ProductVariant> ProductVariants, int TotalCount)> GetPagedProductVariantsAsync(
            int pageNumber,
            int pageSize,
            string productId = null,
            string rarityId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true)
        {
            var query = _dbContext.ProductVariants
                .Include(pv => pv.Product)
                .Include(pv => pv.Rarity)
                .AsQueryable();

            // Filter by product
            if (!string.IsNullOrWhiteSpace(productId))
            {
                query = query.Where(pv => pv.ProductId == productId);
            }

            // Filter by rarity
            if (!string.IsNullOrWhiteSpace(rarityId))
            {
                query = query.Where(pv => pv.RarityId == rarityId);
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(pv => pv.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(pv => pv.Price <= maxPrice.Value);
            }

            // Apply search if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(pv =>
                    pv.Product.Name.ToLower().Contains(searchTerm) ||
                    pv.Rarity.Name.ToLower().Contains(searchTerm));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "price":
                        query = ascending
                            ? query.OrderBy(pv => pv.Price)
                            : query.OrderByDescending(pv => pv.Price);
                        break;
                    case "stock":
                        query = ascending
                            ? query.OrderBy(pv => pv.StockQuantity)
                            : query.OrderByDescending(pv => pv.StockQuantity);
                        break;
                    case "product":
                        query = ascending
                            ? query.OrderBy(pv => pv.Product.Name)
                            : query.OrderByDescending(pv => pv.Product.Name);
                        break;
                    case "rarity":
                        query = ascending
                            ? query.OrderBy(pv => pv.Rarity.Name)
                            : query.OrderByDescending(pv => pv.Rarity.Name);
                        break;
                    default:
                        query = ascending
                            ? query.OrderBy(pv => pv.Product.Name).ThenBy(pv => pv.Rarity.Name)
                            : query.OrderByDescending(pv => pv.Product.Name).ThenByDescending(pv => pv.Rarity.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(pv => pv.Product.Name).ThenBy(pv => pv.Rarity.Name);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var productVariants = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (productVariants, totalCount);
        }

        public override async Task<ProductVariant> GetByIdAsync(string id)
        {
            return await _dbContext.ProductVariants
                .Include(pv => pv.Product)
                .Include(pv => pv.Rarity)
                .FirstOrDefaultAsync(pv => pv.VariantId == id);
        }
    }
}