using NashTech_TCG_ShareViewModels.ViewModels;

namespace NashTech_TCG_MVC.Services.Interfaces
{
    public interface IOrderService
    {
        Task<(bool Success, string Message, string OrderId)> PlaceOrderAsync(PlaceOrderViewModel model);
        Task<(bool Success, string Message, OrderViewModel Data)> GetOrderByIdAsync(string orderId);
    }
}