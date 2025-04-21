using System;
using System.Collections.Generic;
using System.Text;

namespace NashTech_TCG_ShareViewModels.ViewModels
{
    public class ProductViewModel
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string PriceRange => MinPrice.HasValue && MaxPrice.HasValue ?
            (MinPrice == MaxPrice ? $"${MinPrice}" : $"${MinPrice} - ${MaxPrice}") : "N/A";
        public DateTime CreatedDate { get; set; }

        // Product details specific properties
        public IEnumerable<ProductVariantViewModel> Variants { get; set; }
        public IEnumerable<ProductRatingViewModel> Ratings { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
    }

    public class ProductVariantViewModel
    {
        public string VariantId { get; set; }
        public string ProductId { get; set; }
        public string RarityId { get; set; }
        public string RarityName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ProductRatingViewModel
    {
        public string RatingId { get; set; }
        public string ProductId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class PagedProductViewModel
    {
        public IEnumerable<ProductViewModel> Products { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public string CategoryId { get; set; }
        public string SearchTerm { get; set; }
        public string SortBy { get; set; }
        public bool Ascending { get; set; }
    }
}
