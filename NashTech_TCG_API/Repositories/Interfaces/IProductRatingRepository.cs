using NashTech_TCG_API.Models;

namespace NashTech_TCG_API.Repositories.Interfaces
{
    public interface IProductRatingRepository : IRepository<ProductRating>
    {
        Task<IEnumerable<ProductRating>> GetRatingsByProductIdAsync(string productId);
    }
}
