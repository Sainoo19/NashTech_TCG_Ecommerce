using NashTech_TCG_ShareViewModels.ViewModels;

namespace NashTech_TCG_API.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderViewModel> CreateOrderAsync(string userId, PlaceOrderViewModel model);
        Task<OrderViewModel> GetOrderByIdAsync(string orderId, string userId);
    }
}
