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

                var (success, message, data) = await _productService.GetProductDetailsAsync(id);

                if (success)
                {
                    return View(data);
                }

                _logger.LogWarning($"Failed to retrieve product {id}: {message}");
                return View("Error", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product {id}");
                return View("Error", "An error occurred while retrieving the product.");
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
                // Redirect to login page with return URL to this product
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Details", "Product", new { id = model.ProductId }) });
            }

            if (!ModelState.IsValid)
            {
                // Return to product details with error
                TempData["ErrorMessage"] = "Please provide a valid rating.";
                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }

            try
            {
                var (success, message, data) = await _productService.AddProductRatingAsync(model);

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
                TempData["ErrorMessage"] = "An error occurred while submitting your rating.";
                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }
        }

    }
}