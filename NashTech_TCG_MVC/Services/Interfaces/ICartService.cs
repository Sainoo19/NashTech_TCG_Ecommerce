using NashTech_TCG_ShareViewModels.ViewModels;

namespace NashTech_TCG_MVC.Services.Interfaces
{
    public interface ICartService
    {
        Task<(bool Success, string Message, CartViewModel Data)> GetCartAsync();
        Task<(bool Success, string Message, CartItemViewModel Data)> AddToCartAsync(AddToCartViewModel model);
        Task<(bool Success, string Message, CartItemViewModel Data)> UpdateCartItemAsync(UpdateCartItemViewModel model);
        Task<(bool Success, string Message)> RemoveCartItemAsync(string cartItemId);
        Task<(bool Success, string Message)> ClearCartAsync();
        
    }
}
