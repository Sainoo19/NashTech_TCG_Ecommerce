using System;
using System.Collections.Generic;
using System.Text;

namespace NashTech_TCG_ShareViewModels.ViewModels
{
    public class OrderViewModel
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public ShippingAddressViewModel ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public List<OrderItemViewModel> Items { get; set; }
    }

    public class OrderItemViewModel
    {
        public string OrderItemId { get; set; }
        public string VariantId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public string RarityName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
    }
}
