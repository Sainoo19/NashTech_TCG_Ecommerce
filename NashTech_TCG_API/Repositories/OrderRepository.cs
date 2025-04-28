using Microsoft.EntityFrameworkCore;
using NashTech_TCG_API.Data;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;
using NashTech_TCG_API.Utilities;
using NashTech_TCG_ShareViewModels.ViewModels;
using System.Linq.Expressions;

namespace NashTech_TCG_API.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderAsync(Order order, ShippingAddressViewModel shippingAddress)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Add order to context
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                // Create shipping address
                var address = new ShippingAddress
                {
                    OrderId = order.OrderId,
                    FullName = shippingAddress.FullName,
                    PhoneNumber = shippingAddress.PhoneNumber,
                    Email = shippingAddress.Email,
                    AddressLine = shippingAddress.AddressLine,
                    City = shippingAddress.City,
                    Province = shippingAddress.Province,
                    PostalCode = shippingAddress.PostalCode,
                    Country = shippingAddress.Country
                };

                await _context.ShippingAddresses.AddAsync(address);
                await _context.SaveChangesAsync();

                // Update inventory for each order item
                foreach (var item in order.OrderItems)
                {
                    var variant = await _context.ProductVariants.FindAsync(item.VariantId);
                    if (variant != null)
                    {
                        variant.StockQuantity -= item.Quantity;
                        _context.ProductVariants.Update(variant);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return order;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Order> GetOrderByIdAsync(string orderId, string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Rarity)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);
        }

        public async Task<ShippingAddressViewModel> GetShippingAddressForOrderAsync(string orderId)
        {
            var address = await _context.ShippingAddresses
                .FirstOrDefaultAsync(a => a.OrderId == orderId);

            if (address == null)
            {
                return null;
            }

            return new ShippingAddressViewModel
            {
                FullName = address.FullName,
                PhoneNumber = address.PhoneNumber,
                Email = address.Email,
                AddressLine = address.AddressLine,
                City = address.City,
                Province = address.Province,
                PostalCode = address.PostalCode,
                Country = address.Country
            };
        }

        public async Task<IEnumerable<Product>> GetBestSellingProductsAsync(int limit = 8)
        {
            try
            {
                // First, get the product IDs and their total quantities
                var bestSellingProductIds = await _context.OrderItems
                    .GroupBy(oi => oi.ProductVariant.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalQuantity = g.Sum(oi => oi.Quantity)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(limit)
                    .Select(x => x.ProductId)
                    .ToListAsync();

                // Then fetch the complete product data using the IDs
                var bestSellingProducts = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => bestSellingProductIds.Contains(p.ProductId))
                    .ToListAsync();

                // Preserve the original ordering
                return bestSellingProductIds
                    .Select(id => bestSellingProducts.First(p => p.ProductId == id))
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving best selling products", ex);
            }
        }


    }
}