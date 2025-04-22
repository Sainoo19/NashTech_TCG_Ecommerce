using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace NashTech_TCG_ShareViewModels.ViewModels
{
    public class CartViewModel
    {
        public string CartId { get; set; }
        public string UserId { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal TotalPrice => Items.Sum(i => i.Subtotal);
        public int TotalItems => Items.Sum(i => i.Quantity);
    }



    public class AddToCartViewModel
    {
        [Required(ErrorMessage = "Product variant ID is required")]
        public string VariantId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        // Add this property to store the product ID
        public string ProductId { get; set; }
    }


    public class UpdateCartItemViewModel
    {
        [Required(ErrorMessage = "Cart item ID is required")]
        public string CartItemId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
