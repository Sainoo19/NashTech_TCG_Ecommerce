using System.ComponentModel.DataAnnotations;

namespace NashTech_TCG_API.Models.DTOs
{

    public class ProductDTO
    {
        private string FormatPrice(decimal price)
        {
            // Format with thousands separator (dot in Vietnamese format) and no decimal places
            return price.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
        }
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string PriceRange => MinPrice.HasValue && MaxPrice.HasValue ?
     (MinPrice == MaxPrice ? $"{FormatPrice(MinPrice.Value)}đ" : $"{FormatPrice(MinPrice.Value)}đ - {FormatPrice(MaxPrice.Value)}đ") : "N/A";
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

    }

    public class CreateProductDTO
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Category ID is required")]
        public string CategoryId { get; set; }

        public string Description { get; set; }

        // Remove original ImageUrl property
        // [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        // public string ImageUrl { get; set; }

        // Add file upload property
        [Display(Name = "Product Image")]
        public IFormFile ImageFile { get; set; }
    }

    public class UpdateProductDTO
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Category ID is required")]
        public string CategoryId { get; set; }

        public string Description { get; set; }

        

        // Add file upload property
        [Display(Name = "Product Image")]
        public IFormFile ImageFile { get; set; }
    }

    public class ProductFilterDTO
    {
        public string CategoryId { get; set; }
        public string SearchTerm { get; set; }
        public string SortBy { get; set; }
        public bool Ascending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
