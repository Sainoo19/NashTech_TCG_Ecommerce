using Microsoft.AspNetCore.Mvc;
using NashTech_TCG_MVC.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;
using NashTech_TCG_MVC.Models.Responses;

namespace NashTech_TCG_MVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            string categoryId = null,
            string searchTerm = null,
            string sortBy = "name",
            bool ascending = true,
            int pageNumber = 1,
            int pageSize = 9)
        {
            try
            {
                // Fetch products
                var (success, message, data) = await _productService.GetPagedProductsAsync(
                    categoryId, searchTerm, sortBy, ascending, pageNumber, pageSize);

                if (!success)
                {
                    _logger.LogWarning($"Failed to retrieve products: {message}");
                    return View("Error", message);
                }

                // Fetch categories for the filter dropdown
                var (categoriesSuccess, categoriesMessage, categories) = await _productService.GetAllCategoriesAsync();
                if (categoriesSuccess)
                {
                    ViewBag.Categories = categories;
                }
                else
                {
                    _logger.LogWarning($"Failed to retrieve categories: {categoriesMessage}");
                    ViewBag.Categories = new List<CategoryViewModel>();
                }

                return View(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return View("Error", "An error occurred while retrieving products.");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest();
                }

                var (success, message, product) = await _productService.GetProductDetailsAsync(id);

                if (!success)
                {
                    _logger.LogWarning($"Failed to retrieve product {id}: {message}");
                    return View("Error", message);
                }

                // Prepare rating statistics for the view
                var ratingStats = new Dictionary<int, int>();
                var ratingPercentages = new Dictionary<int, int>();

                for (int i = 5; i >= 1; i--)
                {
                    int count = product.Ratings?.Count(r => r.Rating == i) ?? 0;
                    int percentage = product.RatingCount > 0 ? (count * 100) / product.RatingCount : 0;

                    ratingStats[i] = count;
                    ratingPercentages[i] = percentage;
                }

                ViewBag.RatingStats = ratingStats;
                ViewBag.RatingPercentages = ratingPercentages;

                // Format all prices in Vietnamese style
                ViewBag.FormatPrice = new Func<decimal, string>(price =>
                    price.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".") + "đ");

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product {id}");
                return View("Error", "An error occurred while retrieving the product.");
            }
        }

        // Add new method to handle tab activation and review filtering
        [HttpGet]
        public async Task<IActionResult> GetProductTab(string id, string tab, int? filterRating = null)
        {
            var (success, _, product) = await _productService.GetProductDetailsAsync(id);

            if (!success)
            {
                return PartialView("_ErrorPartial", "Failed to load product data");
            }

            if (tab.Equals("reviews", StringComparison.OrdinalIgnoreCase))
            {
                // Filter ratings if needed
                if (filterRating.HasValue)
                {
                    product.Ratings = product.Ratings.Where(r => r.Rating == filterRating.Value);
                }

                return PartialView("_ReviewsTab", product);
            }
            else if (tab.Equals("details", StringComparison.OrdinalIgnoreCase))
            {
                return PartialView("_DetailsTab", product);
            }
            else // Default to description
            {
                return PartialView("_DescriptionTab", product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            string searchTerm = null,
            string categoryId = null,
            string sortBy = "name",
            bool ascending = true,
            int pageNumber = 1,
            int pageSize = 9)
        {
            try
            {
                var (success, message, data) = await _productService.GetPagedProductsAsync(
                    categoryId, searchTerm, sortBy, ascending, pageNumber, pageSize);

                if (success)
                {
                    return PartialView("_ProductGrid", data);
                }

                _logger.LogWarning($"Failed to search products: {message}");
                return PartialView("_ErrorPartial", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                return PartialView("_ErrorPartial", "An error occurred while searching products.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddRating(ProductRatingInputViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "You must be logged in to submit a review." });
                }

                // Redirect to login page with return URL to this product
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Details", "Product", new { id = model.ProductId }) });
            }

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Please provide a valid rating." });
                }

                // Return to product details with error
                TempData["ErrorMessage"] = "Please provide a valid rating.";
                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }

            try
            {
                var (success, message, data) = await _productService.AddProductRatingAsync(model);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success, message, data });
                }

                if (success)
                {
                    TempData["SuccessMessage"] = "Your rating has been submitted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }

                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding rating for product {model.ProductId}");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while submitting your review." });
                }

                TempData["ErrorMessage"] = "An error occurred while submitting your rating.";
                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }
        }
        // Add/update this method in ../NashTech_TCG_MVC/Controllers/ProductController.cs
        [HttpGet]
        public async Task<IActionResult> GetRelatedProducts(string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(productId))
                {
                    _logger.LogWarning("GetRelatedProducts called with empty productId");
                    return PartialView("_ErrorPartial", "Product ID is required");
                }

                _logger.LogInformation($"Getting related products for product ID: {productId}");

                var (success, message, relatedProducts) = await _productService.GetRelatedProductsAsync(productId);

                if (!success || relatedProducts == null)
                {
                    _logger.LogWarning($"Failed to retrieve related products: {message}");
                    return PartialView("_ErrorPartial", message ?? "Failed to retrieve related products");
                }

                _logger.LogInformation($"Successfully retrieved {relatedProducts.Count()} related products");
                return PartialView("_RelatedProductsSlider", relatedProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving related products for product {productId}");
                return PartialView("_ErrorPartial", "An error occurred while retrieving related products");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBestSellingProducts(int limit = 8)
        {
            try
            {
                var (success, message, data) = await _productService.GetBestSellingProductsAsync(limit);

                if (!success || data == null)
                {
                    _logger.LogWarning($"Failed to retrieve best selling products: {message}");
                    return PartialView("_ErrorPartial", message);
                }

                return PartialView("_BestSellingProductsSlider", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving best selling products");
                return PartialView("_ErrorPartial", "An error occurred while retrieving best selling products.");
            }
        }

    }

}
