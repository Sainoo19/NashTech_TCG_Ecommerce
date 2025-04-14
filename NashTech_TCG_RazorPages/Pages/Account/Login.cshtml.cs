using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NashTech_TCG_RazorPages.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;

namespace NashTech_TCG_RazorPages.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginViewModel Input { get; set; } = new LoginViewModel();

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public void OnGet(string returnUrl = null)
        {
            // Clear existing external cookie
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            returnUrl = returnUrl ?? Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var (success, message, token) = await _authService.LoginAsync(Input);

            if (!success)
            {
                ErrorMessage = message;
                return Page();
            }

            // Store the token
            HttpContext.Session.SetString("AccessToken", token);

            // Create claims identity from the token
            var claimsPrincipal = await _authService.ValidateTokenAsync(token);
            if (claimsPrincipal == null)
            {
                ErrorMessage = "Failed to validate token";
                return Page();
            }

            // Get user roles
            var roles = await _authService.GetUserRolesAsync(token);
            if (roles != null && roles.Contains("Admin"))
            {
                // Redirect admin users to the React admin app
                return Redirect("https://localhost:5002");
            }

            // Sign in the user
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                new AuthenticationProperties
                {
                    IsPersistent = Input.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                });

            SuccessMessage = "Login successful!";
            return LocalRedirect(returnUrl);
        }
    }
}
