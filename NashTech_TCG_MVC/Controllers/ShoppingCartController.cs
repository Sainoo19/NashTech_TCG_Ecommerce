using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using NashTech_TCG_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NashTech_TCG_ShareViewModels.ViewModels;

namespace NashTech_TCG_MVC.Controllers
{
    
    public class ShoppingCartController : Controller
    {
        private readonly ICartService _cartService;

        public ShoppingCartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _cartService.GetCartAsync();

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Redirect to login page with return URL to this product
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Details", "Product", new { id = model.ProductId }) });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid model state" });
            }

            var result = await _cartService.AddToCartAsync(model);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = result.Success, message = result.Message, item = result.Data });
            }

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                // Use ProductId for redirect if available
                if (!string.IsNullOrEmpty(model.ProductId))
                {
                    return RedirectToAction("Details", "Product", new { id = model.ProductId });
                }
                return RedirectToAction("Index", "Product");
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(UpdateCartItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid model state" });
            }

            var result = await _cartService.UpdateCartItemAsync(model);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = result.Success, message = result.Message, item = result.Data });
            }

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = result.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCartItem(string cartItemId)
        {
            var result = await _cartService.RemoveCartItemAsync(cartItemId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = result.Success, message = result.Message });
            }

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = result.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var result = await _cartService.ClearCartAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = result.Success, message = result.Message });
            }

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = result.Message;
            }

            return RedirectToAction("Index");
        }

        // Add this method to ShoppingCartController.cs
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var result = await _cartService.GetCartAsync();

            if (result.Success)
            {
                int count = result.Data?.TotalItems ?? 0;
                return Json(new { success = true, count = count });
            }

            return Json(new { success = false, count = 0 });
        }

    }
}
