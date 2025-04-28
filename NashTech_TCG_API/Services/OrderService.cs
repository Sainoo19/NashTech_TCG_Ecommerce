using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;

namespace NashTech_TCG_API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task<OrderViewModel> CreateOrderAsync(string userId, PlaceOrderViewModel model)
        {
            try
            {
                // Get cart items
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null || !cart.CartItems.Any())
                {
                    throw new InvalidOperationException("Cart is empty");
                }

                // Calculate total amount
                decimal totalAmount = cart.CartItems.Sum(item =>
                    item.ProductVariant.Price * item.Quantity);

                // Create order with address information
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Status = model.PaymentMethod == "COD" ? "Pending" : "Awaiting Payment"
                };

                // Add order items from cart
                foreach (var cartItem in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        VariantId = cartItem.VariantId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.ProductVariant.Price
                    };
                    order.OrderItems.Add(orderItem);
                }

                // Save order and address in database
                var createdOrder = await _orderRepository.CreateOrderAsync(order, model.ShippingAddress);

                // Clear the cart after successful order creation
                await _cartRepository.ClearCartAsync(userId);

                // Map to view model and return
                return MapOrderToViewModel(createdOrder, model.ShippingAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                throw;
            }
        }

        public async Task<OrderViewModel> GetOrderByIdAsync(string orderId, string userId)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId, userId);
                if (order == null)
                {
                    return null;
                }

                var shippingAddress = await _orderRepository.GetShippingAddressForOrderAsync(orderId);
                return MapOrderToViewModel(order, shippingAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId} for user {UserId}", orderId, userId);
                throw;
            }
        }

        private OrderViewModel MapOrderToViewModel(Order order, ShippingAddressViewModel shippingAddress)
        {
            return new OrderViewModel
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = shippingAddress,
                PaymentMethod = order.PaymentMethod != null ?
                    order.PaymentMethod : "COD",
                Items = order.OrderItems.Select(item => new OrderItemViewModel
                {
                    OrderItemId = item.OrderItemId,
                    VariantId = item.VariantId,
                    ProductId = item.ProductVariant.ProductId,
                    ProductName = item.ProductVariant.Product.Name,
                    ProductImageUrl = item.ProductVariant.Product.ImageUrl,
                    RarityName = item.ProductVariant.Rarity.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };
        }
    }
}