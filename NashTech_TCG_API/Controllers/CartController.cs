using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;
using NashTech_TCG_API.Common;
using System.Net;

namespace NashTech_TCG_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet]      
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userId = User.FindFirstValue(Claims.Subject)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "User identifier not found in claims",
                        (int)HttpStatusCode.BadRequest));
                }

                var cart = await _cartService.GetCartByUserIdAsync(userId);
                return Ok(ApiResponse<CartViewModel>.SuccessResponse(cart, "Cart retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving cart"));
            }
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("items")]
        [ProducesResponseType(typeof(ApiResponse<ProductRatingViewModel>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<object>.ErrorResponse(errors));
                }

                var userId = User.FindFirstValue(Claims.Subject)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "User identifier not found in claims",
                        (int)HttpStatusCode.BadRequest));
                }

                var cartItem = await _cartService.AddToCartAsync(userId, model);
                return Ok(ApiResponse<CartItemViewModel>.SuccessResponse(cartItem, "Item added to cart successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while adding item to cart"));
            }
        }

        [HttpPut("items")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<object>.ErrorResponse(errors));
                }

                var userId = User.FindFirstValue(Claims.Subject)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "User identifier not found in claims",
                        (int)HttpStatusCode.BadRequest));
                }

                var updatedItem = await _cartService.UpdateCartItemAsync(userId, model);
                if (updatedItem == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Cart item not found"));
                }

                return Ok(ApiResponse<CartItemViewModel>.SuccessResponse(updatedItem, "Cart item updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating cart item"));
            }
        }

        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItem(string cartItemId)
        {
            try
            {
                var userId = User.FindFirstValue(Claims.Subject)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "User identifier not found in claims",
                        (int)HttpStatusCode.BadRequest));
                }

                var result = await _cartService.RemoveCartItemAsync(userId, cartItemId);
                if (!result)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Cart item not found"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(null, "Cart item removed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while removing cart item"));
            }
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = User.FindFirstValue(Claims.Subject)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "User identifier not found in claims",
                        (int)HttpStatusCode.BadRequest));
                }

                var result = await _cartService.ClearCartAsync(userId);
                if (!result)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Cart not found"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(null, "Cart cleared successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while clearing cart"));
            }
        }
    }
}
