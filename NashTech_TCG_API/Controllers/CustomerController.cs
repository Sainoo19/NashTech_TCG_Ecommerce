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
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [HttpGet]
        //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCustomers(
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

                var (customers, totalCount, totalPages) = await _customerService.GetPagedCustomersAsync(
                    pageNumber, pageSize, searchTerm, sortBy, ascending);

                var paginatedResult = new PagedResultDTO<CustomerDTO>
                {
                    Items = customers,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Ok(ApiResponse<PagedResultDTO<CustomerDTO>>.SuccessResponse(
                    paginatedResult,
                    "Customers retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving customers"));
            }
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCustomer(string id)
        {
            try
            {
                var customer = await _customerService.GetByIdAsync(id);

                if (customer == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Customer with ID {id} not found", 404));

                return Ok(ApiResponse<CustomerDTO>.SuccessResponse(customer, "Customer retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving customer {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving customer"));
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> CreateCustomer(CreateCustomerDTO customerDTO)
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

                var createdCustomer = await _customerService.CreateCustomerAsync(customerDTO);

                if (createdCustomer == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Failed to create customer"));
                }

                return CreatedAtAction(
                    nameof(GetCustomer),
                    new { id = createdCustomer.Id },
                    ApiResponse<CustomerDTO>.SuccessResponse(createdCustomer, "Customer created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating customer"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateCustomer(string id, UpdateCustomerDTO customerDTO)
        {
            try
            {
                // Check if current user is admin or the customer themselves
                if (!User.IsInRole("Admin") && User.FindFirst("sub")?.Value != id)
                {
                    return Forbid();
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<object>.ErrorResponse(errors));
                }

                var updatedCustomer = await _customerService.UpdateCustomerAsync(id, customerDTO);

                if (updatedCustomer == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Customer with ID {id} not found", 404));

                return Ok(ApiResponse<CustomerDTO>.SuccessResponse(updatedCustomer, "Customer updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating customer {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating customer"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);

                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Customer with ID {id} not found", 404));

                return Ok(ApiResponse<object>.SuccessResponse(null, "Customer deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting customer {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting customer"));
            }
        }
    }
}
