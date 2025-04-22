using Microsoft.EntityFrameworkCore;
using NashTech_TCG_API.Data;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;
using NashTech_TCG_API.Utilities;

namespace NashTech_TCG_API.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;
        private readonly IdGenerator _idGenerator;

        public CartRepository(AppDbContext context, IdGenerator idGenerator)
        {
            _context = context;
            _idGenerator = idGenerator;
        }

        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            // Get cart with all items and their related product variants
            return await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Rarity)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<CartItem> GetCartItemByIdAsync(string cartItemId)
        {
            return await _context.CartItems
                .Include(ci => ci.ProductVariant)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
        }

        
        public async Task<CartItem> AddItemToCartAsync(string userId, string variantId, int quantity)
        {
            // First check if the user has a cart
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            string cartId = await _idGenerator.GenerateId("CART");
            string cartItemId = await _idGenerator.GenerateId("ITEM");

            // If no cart, create one
            if (cart == null)
            {
                cart = new Cart
                {
                    CartId = cartId,
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
            }

            // Check if the item is already in the cart
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.VariantId == variantId);

            if (existingItem != null)
            {
                // Update quantity of existing item
                existingItem.Quantity += quantity;
                _context.CartItems.Update(existingItem);
                await _context.SaveChangesAsync();

                // Reload the cart item with all necessary relationships
                return await _context.CartItems
                    .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                    .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.Rarity)
                    .FirstAsync(ci => ci.CartItemId == existingItem.CartItemId);
            }
            else
            {
                // Add new item to cart
                var cartItem = new CartItem
                {
                    CartItemId = cartItemId,
                    CartId = cart.CartId,
                    VariantId = variantId,
                    Quantity = quantity
                };

                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();

                // Reload the cart item with all necessary relationships
                return await _context.CartItems
                    .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                    .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.Rarity)
                    .FirstAsync(ci => ci.CartItemId == cartItem.CartItemId);
            }
        }


        public async Task<CartItem> UpdateCartItemAsync(string cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null) return null;

            cartItem.Quantity = quantity;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();

            // Return with all relationships included
            return await _context.CartItems
                .Include(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .Include(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Rarity)
                .FirstAsync(ci => ci.CartItemId == cartItemId);
        }


        public async Task<bool> RemoveCartItemAsync(string cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null) return false;

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return false;

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
