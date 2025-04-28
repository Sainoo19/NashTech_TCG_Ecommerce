using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashTech_TCG_API.Common;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Net;
using System.Security.Claims;

namespace NashTech_TCG_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] PlaceOrderViewModel model)
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

                var order = await _orderService.CreateOrderAsync(userId, model);
                return Ok(ApiResponse<OrderViewModel>.SuccessResponse(order, "Order created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating order"));
            }
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(string orderId)
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

                var order = await _orderService.GetOrderByIdAsync(orderId, userId);

                if (order == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Order not found"));
                }

                return Ok(ApiResponse<OrderViewModel>.SuccessResponse(order, "Order retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving order"));
            }
        }
    }
}