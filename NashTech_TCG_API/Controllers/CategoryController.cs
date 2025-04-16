using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashTech_TCG_API.Common;
using NashTech_TCG_API.Models.DTOs;
using NashTech_TCG_API.Services.Interfaces;
using OpenIddict.Validation.AspNetCore;

namespace NashTech_TCG_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories(
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

                var (categories, totalCount, totalPages) = await _categoryService.GetPagedCategoriesAsync(
                    pageNumber, pageSize, searchTerm, sortBy, ascending);

                var paginatedResult = new PagedResultDTO<CategoryDTO>
                {
                    Items = categories,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Ok(ApiResponse<PagedResultDTO<CategoryDTO>>.SuccessResponse(
                    paginatedResult,
                    "Categories retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving categories"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(string id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);

                if (category == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Category with ID {id} not found", 404));

                return Ok(ApiResponse<CategoryDTO>.SuccessResponse(category, "Category retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving category {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving category"));
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateCategory(CreateCategoryDTO categoryDTO)
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

                var createdCategory = await _categoryService.CreateCategoryAsync(categoryDTO);

                return CreatedAtAction(
                    nameof(GetCategory),
                    new { id = createdCategory.CategoryId },
                    ApiResponse<CategoryDTO>.SuccessResponse(createdCategory, "Category created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating category"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateCategory(string id, UpdateCategoryDTO categoryDTO)
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

                var updatedCategory = await _categoryService.UpdateCategoryAsync(id, categoryDTO);

                if (updatedCategory == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Category with ID {id} not found", 404));

                return Ok(ApiResponse<CategoryDTO>.SuccessResponse(updatedCategory, "Category updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating category {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating category"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);

                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Category with ID {id} not found", 404));

                return Ok(ApiResponse<object>.SuccessResponse(null, "Category deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting category {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting category"));
            }
        }
    }
}
