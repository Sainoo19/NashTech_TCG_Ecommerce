using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashTech_TCG_API.Models.DTOs;
using NashTech_TCG_API.Services.Interfaces;
using OpenIddict.Validation.AspNetCore;
using NashTech_TCG_API.Common;
using NashTech_TCG_API.Services;

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
            [FromQuery] bool ascending = true)
        {
            try
            {
                if (pageNumber < 1)
                    pageNumber = 1;

                if (pageSize < 1 || pageSize > 100)
                    pageSize = 10;

                var (products, totalCount, totalPages) = await _productService.GetPagedProductsAsync(
                    pageNumber, pageSize, categoryId, searchTerm, sortBy, ascending);

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



    }
}