using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashTech_TCG_API.Models.DTOs;
using NashTech_TCG_API.Common;
using NashTech_TCG_API.Services.Interfaces;
using OpenIddict.Validation.AspNetCore;

namespace NashTech_TCG_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RarityController : ControllerBase
    {
        private readonly IRarityService _rarityService;
        private readonly ILogger<RarityController> _logger;

        public RarityController(IRarityService rarityService, ILogger<RarityController> logger)
        {
            _rarityService = rarityService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetRarities(
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

                var (rarities, totalCount, totalPages) = await _rarityService.GetPagedRaritiesAsync(
                    pageNumber, pageSize, searchTerm, sortBy, ascending);

                var paginatedResult = new PagedResultDTO<RarityDTO>
                {
                    Items = rarities,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Ok(ApiResponse<PagedResultDTO<RarityDTO>>.SuccessResponse(
                    paginatedResult,
                    "Rarities retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rarities");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving rarities"));
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllRarities()
        {
            try
            {
                var rarities = await _rarityService.GetAllAsync();
                return Ok(ApiResponse<IEnumerable<RarityDTO>>.SuccessResponse(
                    rarities,
                    "All rarities retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all rarities");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving all rarities"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRarity(string id)
        {
            try
            {
                var rarity = await _rarityService.GetByIdAsync(id);

                if (rarity == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Rarity with ID {id} not found", 404));

                return Ok(ApiResponse<RarityDTO>.SuccessResponse(rarity, "Rarity retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving rarity {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving rarity"));
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateRarity(CreateRarityDTO rarityDTO)
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

                var createdRarity = await _rarityService.CreateRarityAsync(rarityDTO);

                return CreatedAtAction(
                    nameof(GetRarity),
                    new { id = createdRarity.RarityId },
                    ApiResponse<RarityDTO>.SuccessResponse(createdRarity, "Rarity created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rarity");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating rarity"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateRarity(string id, UpdateRarityDTO rarityDTO)
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

                var updatedRarity = await _rarityService.UpdateRarityAsync(id, rarityDTO);

                if (updatedRarity == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Rarity with ID {id} not found", 404));

                return Ok(ApiResponse<RarityDTO>.SuccessResponse(updatedRarity, "Rarity updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating rarity {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating rarity"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteRarity(string id)
        {
            try
            {
                var result = await _rarityService.DeleteRarityAsync(id);

                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Rarity with ID {id} not found", 404));

                return Ok(ApiResponse<object>.SuccessResponse(null, "Rarity deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting rarity {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting rarity"));
            }
        }
    }
}
