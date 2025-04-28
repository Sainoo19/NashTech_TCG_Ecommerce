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
        private readonly IOrderService _orderService;

        public ShoppingCartController(ICartService cartService, IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
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

        
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cartResult = await _cartService.GetCartAsync();

            if (!cartResult.Success)
            {
                TempData["ErrorMessage"] = cartResult.Message;
                return RedirectToAction("Index");
            }

            if (cartResult.Data == null || !cartResult.Data.Items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty";
                return RedirectToAction("Index");
            }

            var checkoutViewModel = new CheckoutViewModel
            {
                Cart = cartResult.Data,
                ShippingAddress = new ShippingAddressViewModel()
            };

            return View(checkoutViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> PlaceOrder(PlaceOrderViewModel model)
        {
            // Kiểm tra model trước khi gửi đến API
            Console.WriteLine($"ShippingAddress is null? {model.ShippingAddress == null}");
            Console.WriteLine($"PaymentMethod: {model.PaymentMethod}");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                Console.WriteLine($"ModelState errors: {string.Join(", ", errors)}");

                var cartResult = await _cartService.GetCartAsync();
                var checkoutViewModel = new CheckoutViewModel
                {
                    Cart = cartResult.Data,
                    ShippingAddress = model.ShippingAddress,
                    PaymentMethod = model.PaymentMethod
                };

                return View("Checkout", checkoutViewModel);
            }

            var result = await _orderService.PlaceOrderAsync(model);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Checkout");
            }

            if (model.PaymentMethod == "VNPay")
            {
                return RedirectToAction("VNPayCheckout", new { orderId = result.OrderId });
            }

            TempData["SuccessMessage"] = "Your order has been placed successfully!";
            return RedirectToAction("OrderConfirmation", new { orderId = result.OrderId });
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(string orderId)
        {
            var orderResult = await _orderService.GetOrderByIdAsync(orderId);

            if (!orderResult.Success)
            {
                TempData["ErrorMessage"] = orderResult.Message;
                return RedirectToAction("Index", "Home");
            }

            return View(orderResult.Data);
        }

        [Authorize]
        [HttpGet]
        public IActionResult VNPayCheckout(string orderId)
        {
            // This will be implemented in the future
            // For now, just redirect to order confirmation
            TempData["SuccessMessage"] = "Payment simulation complete. VNPay integration will be implemented soon.";
            return RedirectToAction("OrderConfirmation", new { orderId = orderId });
        }
    }
}