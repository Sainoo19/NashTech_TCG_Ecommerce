using Microsoft.EntityFrameworkCore;
using NashTech_TCG_API.Data;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;

namespace NashTech_TCG_API.Repositories
{
    public class ProductRatingRepository : Repository<ProductRating>, IProductRatingRepository
    {
        public ProductRatingRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<ProductRating>> GetRatingsByProductIdAsync(string productId)
        {
            return await _dbContext.ProductRatings
                .Include(r => r.ApplicationUser)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }
    }
}