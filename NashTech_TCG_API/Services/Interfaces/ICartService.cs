using NashTech_TCG_ShareViewModels.ViewModels;

namespace NashTech_TCG_API.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartByUserIdAsync(string userId);
        Task<CartItemViewModel> AddToCartAsync(string userId, AddToCartViewModel model);
        Task<CartItemViewModel> UpdateCartItemAsync(string userId, UpdateCartItemViewModel model);
        Task<bool> RemoveCartItemAsync(string userId, string cartItemId);
        Task<bool> ClearCartAsync(string userId);
    }
}
