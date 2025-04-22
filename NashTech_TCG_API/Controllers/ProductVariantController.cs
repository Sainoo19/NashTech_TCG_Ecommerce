using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashTech_TCG_API.Models.DTOs;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_API.Common;
using OpenIddict.Validation.AspNetCore;
using static NashTech_TCG_API.Models.DTOs.ProductVariantDTOs;

namespace NashTech_TCG_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantService _productVariantService;
        private readonly ILogger<ProductVariantController> _logger;

        public ProductVariantController(IProductVariantService productVariantService, ILogger<ProductVariantController> logger)
        {
            _productVariantService = productVariantService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductVariants(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string productId = null,
            [FromQuery] string rarityId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
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

                var (productVariants, totalCount, totalPages) = await _productVariantService.GetPagedProductVariantsAsync(
                    pageNumber, pageSize, productId, rarityId, minPrice, maxPrice, searchTerm, sortBy, ascending);

                var paginatedResult = new PagedResultDTO<ProductVariantDTO>
                {
                    Items = productVariants,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Ok(ApiResponse<PagedResultDTO<ProductVariantDTO>>.SuccessResponse(
                    paginatedResult,
                    "Product variants retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product variants");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving product variants"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductVariant(string id)
        {
            try
            {
                var productVariant = await _productVariantService.GetByIdAsync(id);

                if (productVariant == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Product variant with ID {id} not found", 404));

                return Ok(ApiResponse<ProductVariantDTO>.SuccessResponse(productVariant, "Product variant retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product variant {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving product variant"));
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateProductVariant([FromForm] CreateProductVariantDTO productVariantDTO)
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

                var createdProductVariant = await _productVariantService.CreateProductVariantAsync(productVariantDTO);

                if (createdProductVariant == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Failed to create product variant. The product or rarity may not exist, or a variant with this combination already exists."));
                }

                return CreatedAtAction(
                    nameof(GetProductVariant),
                    new { id = createdProductVariant.VariantId },
                    ApiResponse<ProductVariantDTO>.SuccessResponse(createdProductVariant, "Product variant created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product variant");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating product variant"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateProductVariant(string id, [FromForm] UpdateProductVariantDTO productVariantDTO)
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

                var updatedProductVariant = await _productVariantService.UpdateProductVariantAsync(id, productVariantDTO);

                if (updatedProductVariant == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Product variant with ID {id} not found", 404));

                return Ok(ApiResponse<ProductVariantDTO>.SuccessResponse(updatedProductVariant, "Product variant updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product variant {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating product variant"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteProductVariant(string id)
        {
            try
            {
                var result = await _productVariantService.DeleteProductVariantAsync(id);

                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Product variant with ID {id} not found", 404));

                return Ok(ApiResponse<object>.SuccessResponse(null, "Product variant deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product variant {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting product variant"));
            }
        }

        [HttpGet("by-product/{productId}")]
        public async Task<IActionResult> GetProductVariantsByProduct(
            string productId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = null,
            [FromQuery] bool ascending = true)
        {
            try
            {
                if (pageNumber < 1)
                    pageNumber = 1;

                if (pageSize < 1 || pageSize > 100)
                    pageSize = 10;

                var (productVariants, totalCount, totalPages) = await _productVariantService.GetPagedProductVariantsAsync(
                    pageNumber, pageSize, productId: productId, sortBy: sortBy, ascending: ascending);

                var paginatedResult = new PagedResultDTO<ProductVariantDTO>
                {
                    Items = productVariants,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Ok(ApiResponse<PagedResultDTO<ProductVariantDTO>>.SuccessResponse(
                    paginatedResult,
                    "Product variants retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product variants for product {productId}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving product variants"));
            }
        }
    }
}
