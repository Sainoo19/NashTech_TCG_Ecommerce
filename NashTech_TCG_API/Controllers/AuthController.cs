using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashTech_TCG_API.Common;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;
using NashTech_TCG_API.Models.DTOs;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System.Net;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.AspNetCore.Authentication;

namespace NashTech_TCG_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<object>.ErrorResponse(errors, "Validation failed"));
                }

                // Log the incoming model data
                Console.WriteLine($"Register attempt for: {model.Email}, FirstName: {model.FirstName}, LastName: {model.LastName}");

                var result = await _authService.RegisterUserAsync(model);

                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        new { registered = true },
                        "User registered successfully"));
                }

                return BadRequest(ApiResponse<object>.ErrorResponse("Failed to register user"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during registration: {ex}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }


        [HttpPost("token")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Token()
        {
            try
            {


                var request = HttpContext.GetOpenIddictServerRequest();
                if (request == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request: OpenIddict server request not found"));
                }

                if (request.IsPasswordGrantType())
                {
                    var user = await _authService.ValidateUserAsync(request.Username, request.Password);
                    if (user == null)
                    {
                        return Unauthorized(ApiResponse<object>.ErrorResponse(
                            "Invalid credentials",
                            (int)HttpStatusCode.Unauthorized));
                    }

                    var principal = await _authService.CreateClaimsPrincipalAsync(user);

                    // Return token immediately without cookie
                    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                return BadRequest(ApiResponse<object>.ErrorResponse("The specified grant type is not supported"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token error: {ex}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    $"Server error: {ex.Message}",
                    (int)HttpStatusCode.InternalServerError));
            }
        }




        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ApiResponse<RolesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserRoles()
        {
            
                var userId = User.FindFirstValue(Claims.Subject)
                    ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userEmail = User.FindFirstValue(Claims.Email)
                    ?? User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userId))
                {
                    var allClaims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
                    var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "User identifier not found in claims",
                        (int)HttpStatusCode.BadRequest));
                }

                var roles = await _authService.GetUserRolesByIdAsync(userId);
                if (roles == null || !roles.Any())
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(
                        "No roles found for user",
                        (int)HttpStatusCode.NotFound));
                }
                var response = new RolesResponse
                {
                    Roles = roles,
                    UserId = userId,
                    Email = userEmail
                };

                return Ok(ApiResponse<RolesResponse>.SuccessResponse(
                    response,
                    "Roles retrieved successfully"));

        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = User.FindFirstValue(Claims.Subject)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "User identifier not found in claims",
                    (int)HttpStatusCode.BadRequest));
            }

            var profile = await _authService.GetUserProfileAsync(userId);
            if (profile == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "User profile not found",
                    (int)HttpStatusCode.NotFound));
            }

            return Ok(ApiResponse<UserProfileViewModel>.SuccessResponse(
                profile,
                "User profile retrieved successfully"));
        }

        [HttpPost("logout")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            // Clear auth cookie
            Response.Cookies.Delete("auth_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Ok(ApiResponse<object>.SuccessResponse(null, "Logged out successfully"));
        }


    }
}
