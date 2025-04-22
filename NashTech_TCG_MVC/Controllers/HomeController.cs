using Microsoft.AspNetCore.Mvc;
using NashTech_TCG_MVC.Models;
using NashTech_TCG_MVC.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;
using System.Diagnostics;

namespace NashTech_TCG_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IHomeService homeService, ILogger<HomeController> logger)
        {
            _homeService = homeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Fetch all categories for category buttons
                var (categoriesSuccess, categoriesMessage, categories) = await _homeService.GetAllCategoriesAsync();

                if (!categoriesSuccess)
                {
                    _logger.LogWarning($"Failed to retrieve categories: {categoriesMessage}");
                    ViewBag.Categories = new List<CategoryViewModel>();
                }
                else
                {
                    ViewBag.Categories = categories;
                }

                // If there are categories, get top rated products for the first category
                if (categoriesSuccess && categories.Any())
                {
                    var firstCategoryId = categories.First().CategoryId;
                    var (productsSuccess, productsMessage, products) = await _homeService.GetTopRatedProductsByCategoryAsync(firstCategoryId);

                    if (productsSuccess)
                    {
                        ViewBag.TopProducts = products;
                        ViewBag.SelectedCategoryId = firstCategoryId;
                        ViewBag.SelectedCategoryName = categories.First().Name;
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to retrieve top products: {productsMessage}");
                        ViewBag.TopProducts = new List<ProductViewModel>();
                    }
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing home page");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTopProductsByCategory(string categoryId)
        {
            try
            {
                if (string.IsNullOrEmpty(categoryId))
                {
                    return BadRequest("Category ID is required");
                }

                var (success, message, products) = await _homeService.GetTopRatedProductsByCategoryAsync(categoryId);

                if (success)
                {
                    return PartialView("_TopProductsSlider", products);
                }
                else
                {
                    _logger.LogWarning($"Failed to retrieve top products for category {categoryId}: {message}");
                    return PartialView("_ErrorPartial", message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving top products for category {categoryId}");
                return PartialView("_ErrorPartial", "An error occurred while retrieving products.");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
