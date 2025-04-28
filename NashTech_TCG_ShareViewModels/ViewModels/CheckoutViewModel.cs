using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NashTech_TCG_ShareViewModels.ViewModels
{
    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; }
        public ShippingAddressViewModel ShippingAddress { get; set; }
        public string PaymentMethod { get; set; } = "COD";
        public decimal TotalAmount => Cart?.TotalPrice ?? 0;
    }

    public class ShippingAddressViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Address line is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string AddressLine { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; }

        [Required(ErrorMessage = "Province/State is required")]
        [StringLength(100, ErrorMessage = "Province/State cannot exceed 100 characters")]
        public string Province { get; set; }

        [StringLength(20, ErrorMessage = "ZIP/Postal code cannot exceed 20 characters")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; } = "Vietnam";
    }

    public class PlaceOrderViewModel
    {
        [Required]
        public ShippingAddressViewModel ShippingAddress { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public string PaymentMethod { get; set; }

        public string Notes { get; set; }
    }
}
