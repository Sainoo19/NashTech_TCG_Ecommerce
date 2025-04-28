using NashTech_TCG_API.Models;
using NashTech_TCG_ShareViewModels.ViewModels;

namespace NashTech_TCG_API.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order, ShippingAddressViewModel shippingAddress);
        Task<Order> GetOrderByIdAsync(string orderId, string userId);
        Task<ShippingAddressViewModel> GetShippingAddressForOrderAsync(string orderId);
        Task<IEnumerable<Product>> GetBestSellingProductsAsync(int limit = 8);

    }
}