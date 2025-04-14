using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NashTech_TCG_RazorPages.Services.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;

namespace NashTech_TCG_RazorPages.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; } = new RegisterViewModel();

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public bool ShowLoginLink { get; set; }

        public void OnGet()
        {
            // Initialize the page
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var (success, message) = await _authService.RegisterAsync(Input);

            if (success)
            {
                SuccessMessage = "Registration successful! You can now login.";
                ShowLoginLink = true;
                Input = new RegisterViewModel(); // Clear the form
                return Page();
            }
            else
            {
                ErrorMessage = message;
                return Page();
            }
        }
    }
}
