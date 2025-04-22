using NashTech_TCG_API.Attributes;
using System.ComponentModel.DataAnnotations;

namespace NashTech_TCG_API.Models.DTOs
{
    public class ProductVariantDTOs
    {
        public class ProductVariantDTO
        {
            public string VariantId { get; set; }
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string RarityId { get; set; }
            public string RarityName { get; set; }
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public string ImageUrl { get; set; }
        }

        public class CreateProductVariantDTO
        {
            [Required(ErrorMessage = "Product ID is required")]
            public string ProductId { get; set; }

            [Required(ErrorMessage = "Rarity ID is required")]
            public string RarityId { get; set; }

            [Required(ErrorMessage = "Price is required")]
            [GreaterThanZero(ErrorMessage = "Price must be greater than 0")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "Stock quantity is required")]
            [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
            public int StockQuantity { get; set; }

            [Display(Name = "Variant Image")]
            public IFormFile? ImageFile { get; set; }
        }

        public class UpdateProductVariantDTO
        {
            [Required(ErrorMessage = "Price is required")]
            [GreaterThanZero(ErrorMessage = "Price must be greater than 0")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "Stock quantity is required")]
            [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
            public int StockQuantity { get; set; }

            [Display(Name = "Variant Image")]
            public IFormFile? ImageFile { get; set; }
        }
    }
}