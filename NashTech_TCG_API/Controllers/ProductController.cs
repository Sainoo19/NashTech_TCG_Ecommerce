using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashTech_TCG_API.Models.DTOs;
using NashTech_TCG_API.Services.Interfaces;
using OpenIddict.Validation.AspNetCore;
using NashTech_TCG_API.Common;
using NashTech_TCG_API.Services;
using NashTech_TCG_ShareViewModels.ViewModels;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Net;

namespace NashTech_TCG_API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
         [FromQuery] string categoryId = null,
         [FromQuery] int pageNumber = 1,
         [FromQuery] int pageSize = 10,
         [FromQuery] string searchTerm = null,
         [FromQuery] string sortBy = null,
         [FromQuery] bool ascending = true,
         [FromQuery] decimal? minPrice = null,
         [FromQuery] decimal? maxPrice = null)
        {
            try
            {
                if (pageNumber < 1)
                    pageNumber = 1;

                if (pageSize < 1 || pageSize > 100)
                    pageSize = 10;

                var (products, totalCount, totalPages) = await _productService.GetPagedProductsAsync(
                    pageNumber, pageSize, categoryId, searchTerm, sortBy, ascending, minPrice, maxPrice);

                var paginatedResult = new PagedResultDTO<ProductDTO>
                {
                    Items = products,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Ok(ApiResponse<PagedResultDTO<ProductDTO>>.SuccessResponse(
                    paginatedResult,
                    "Products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving products"));
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);

                if (product == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Product with ID {id} not found", 404));

                return Ok(ApiResponse<ProductDTO>.SuccessResponse(product, "Product retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving product"));
            }
        }

        [HttpPost]
        //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDTO productDTO)
        {
            _logger.LogInformation($"Received CreateProduct request - Name: {productDTO.Name}, CategoryId: {productDTO.CategoryId}");

            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var state in ModelState)
                    {
                        _logger.LogWarning($"Model state error for {state.Key}: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                    }

                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<object>.ErrorResponse(errors));
                }

                // Log image file details if present
                if (productDTO.ImageFile != null)
                {
                    _logger.LogInformation($"Image file received: {productDTO.ImageFile.FileName}, Size: {productDTO.ImageFile.Length} bytes");
                }
                else
                {
                    _logger.LogInformation("No image file received");
                }

                var createdProduct = await _productService.CreateProductAsync(productDTO);

                if (createdProduct == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid category ID"));

                return CreatedAtAction(
                    nameof(GetProduct),
                    new { id = createdProduct.ProductId },
                    ApiResponse<ProductDTO>.SuccessResponse(createdProduct, "Product created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating product"));
            }
        }

        [HttpPut("{id}")]
        //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProduct(string id, [FromForm] UpdateProductDTO productDTO)
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

                // Log image file details if present
                if (productDTO.ImageFile != null)
                {
                    _logger.LogInformation($"Image file received for update: {productDTO.ImageFile.FileName}, Size: {productDTO.ImageFile.Length} bytes");
                }

                var updatedProduct = await _productService.UpdateProductAsync(id, productDTO);

                if (updatedProduct == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Product with ID {id} not found or invalid category", 404));

                return Ok(ApiResponse<ProductDTO>.SuccessResponse(updatedProduct, "Product updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating product"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);

                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Product with ID {id} not found", 404));

                return Ok(ApiResponse<object>.SuccessResponse(null, "Product deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting product"));
            }
        }

        // New API endpoint for client-facing product display
        [HttpGet("client")]
        public async Task<IActionResult> GetProductsForClient(
            [FromQuery] string categoryId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 9,  // Default to 9 for 3x3 grid
            [FromQuery] string searchTerm = null,
            [FromQuery] string sortBy = null,
            [FromQuery] bool ascending = true)
        {
            try
            {
                if (pageNumber < 1)
                    pageNumber = 1;

                if (pageSize < 1 || pageSize > 100)
                    pageSize = 9;

                var (products, totalCount, totalPages) = await _productService.GetPagedProductsForClientAsync(
                    pageNumber, pageSize, categoryId, searchTerm, sortBy, ascending);

                var paginatedResult = new PagedProductViewModel
                {
                    Products = products,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CategoryId = categoryId,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    Ascending = ascending
                };

                return Ok(ApiResponse<PagedProductViewModel>.SuccessResponse(
                    paginatedResult,
                    "Products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for client");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving products"));
            }
        }

        // Add to ProductController.cs in the API project
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetProductDetails(string id)
        {
            try
            {
                var productDetails = await _productService.GetProductDetailsAsync(id);

                if (productDetails == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Product with ID {id} not found", 404));

                return Ok(ApiResponse<ProductViewModel>.SuccessResponse(productDetails, "Product details retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product details {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving product details"));
            }
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("rating")]
        [ProducesResponseType(typeof(ApiResponse<ProductRatingViewModel>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddRating([FromBody] ProductRatingInputViewModel model)
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


                // Add the rating
                var rating = await _productService.AddProductRatingAsync(model, userId);
                if (rating == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Failed to add rating. Product might not exist."));
                }

                return CreatedAtAction(
                    nameof(GetProductDetails),
                    new { id = model.ProductId },
                    ApiResponse<ProductRatingViewModel>.SuccessResponse(rating, "Rating added successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product rating");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while adding the rating"));
            }
        }


        [HttpGet("category/{categoryId}/top-rated")]
        public async Task<IActionResult> GetTopRatedProductsByCategory(string categoryId,[FromQuery] int limit = 8)
        {
            try
            {
                if (string.IsNullOrEmpty(categoryId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Category ID is required"));
                }

                if (limit <= 0 || limit > 20) // Add reasonable limits
                {
                    limit = 8; // Default to 8 if invalid limit
                }

                var topRatedProducts = await _productService.GetTopRatedProductsByCategoryAsync(categoryId);

                return Ok(ApiResponse<IEnumerable<ProductViewModel>>.SuccessResponse(
                    topRatedProducts,
                    $"Top {limit} rated products for category {categoryId} retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving top rated products for category {categoryId}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving top rated products"));
            }
        }


    }
}