using NashTech_TCG_API.Models;

namespace NashTech_TCG_API.Repositories.Interfaces
{
    public interface ICartRepository 
    {
        Task<Cart> GetCartByUserIdAsync(string userId);
        Task<CartItem> GetCartItemByIdAsync(string cartItemId);
        Task<CartItem> AddItemToCartAsync(string userId, string variantId, int quantity);
        Task<CartItem> UpdateCartItemAsync(string cartItemId, int quantity);
        Task<bool> RemoveCartItemAsync(string cartItemId);
        Task<bool> ClearCartAsync(string userId);
    }
}
