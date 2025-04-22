using NashTech_TCG_API.Repositories.Interfaces;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;

namespace NashTech_TCG_API.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository cartRepository, ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task<CartViewModel> GetCartByUserIdAsync(string userId)
        {
            try
            {
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null)
                {
                    return new CartViewModel
                    {
                        UserId = userId,
                        Items = new List<CartItemViewModel>()
                    };
                }

                var cartViewModel = new CartViewModel
                {
                    CartId = cart.CartId,
                    UserId = cart.UserId,
                    Items = cart.CartItems.Select(item => new CartItemViewModel
                    {
                        CartItemId = item.CartItemId,
                        VariantId = item.VariantId,
                        ProductId = item.ProductVariant.ProductId,
                        ProductName = item.ProductVariant.Product.Name,
                        ProductImageUrl = item.ProductVariant.Product.ImageUrl,
                        RarityName = item.ProductVariant.Rarity.Name,
                        Price = item.ProductVariant.Price,
                        Quantity = item.Quantity,
                        StockQuantity = item.ProductVariant.StockQuantity
                    }).ToList()
                };

                return cartViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for user {UserId}", userId);
                throw;
            }
        }

        public async Task<CartItemViewModel> AddToCartAsync(string userId, AddToCartViewModel model)
        {
            try
            {
                var cartItem = await _cartRepository.AddItemToCartAsync(userId, model.VariantId, model.Quantity);

                // Handle possible null references safely
                if (cartItem == null)
                {
                    throw new InvalidOperationException("Failed to create or retrieve cart item");
                }

                if (cartItem.ProductVariant == null)
                {
                    // Reload the cart item with product variant if it's missing
                    cartItem = await _cartRepository.GetCartItemByIdAsync(cartItem.CartItemId);

                    if (cartItem.ProductVariant == null)
                    {
                        throw new InvalidOperationException($"Product variant with ID {cartItem.VariantId} not found");
                    }
                }

                // Now safely access the properties with null checks
                var productVariant = cartItem.ProductVariant;
                var product = productVariant.Product;
                var rarity = productVariant.Rarity;

                return new CartItemViewModel
                {
                    CartItemId = cartItem.CartItemId,
                    VariantId = cartItem.VariantId,
                    ProductId = productVariant?.ProductId,
                    ProductName = product?.Name ?? "Unknown Product",
                    ProductImageUrl = product?.ImageUrl ?? "",
                    RarityName = rarity?.Name ?? "Unknown Rarity",
                    Price = productVariant?.Price ?? 0,
                    Quantity = cartItem.Quantity,
                    StockQuantity = productVariant?.StockQuantity ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for user {UserId}", userId);
                throw;
            }
        }


        public async Task<CartItemViewModel> UpdateCartItemAsync(string userId, UpdateCartItemViewModel model)
        {
            try
            {
                var cartItem = await _cartRepository.UpdateCartItemAsync(model.CartItemId, model.Quantity);
                if (cartItem == null) return null;

                return new CartItemViewModel
                {
                    CartItemId = cartItem.CartItemId,
                    VariantId = cartItem.VariantId,
                    ProductId = cartItem.ProductVariant.ProductId,
                    ProductName = cartItem.ProductVariant.Product.Name,
                    ProductImageUrl = cartItem.ProductVariant.Product.ImageUrl ?? "",
                    RarityName = cartItem.ProductVariant.Rarity.Name,
                    Price = cartItem.ProductVariant.Price,
                    Quantity = cartItem.Quantity,
                    StockQuantity = cartItem.ProductVariant.StockQuantity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {CartItemId} for user {UserId}", model.CartItemId, userId);
                throw;
            }
        }

        public async Task<bool> RemoveCartItemAsync(string userId, string cartItemId)
        {
            try
            {
                return await _cartRepository.RemoveCartItemAsync(cartItemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item {CartItemId} for user {UserId}", cartItemId, userId);
                throw;
            }
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            try
            {
                return await _cartRepository.ClearCartAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                throw;
            }
        }
    }
}
