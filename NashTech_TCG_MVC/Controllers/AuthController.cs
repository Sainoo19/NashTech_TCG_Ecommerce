using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NashTech_TCG_ShareViewModels.ViewModels;
using NashTech_TCG_MVC.Services.Interfaces;
using System.Security.Claims;
using System;

namespace NashTech_TCG_MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // GET: /Auth/Login
        public IActionResult Login(string returnUrl = null)
        {
            // Xóa cookie xác thực hiện tại
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Lưu URL chuyển hướng
            ViewData["ReturnUrl"] = returnUrl ?? Url.Content("~/");
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;

            // Kiểm tra tính hợp lệ của model
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _logger.LogInformation("Attempting to login user: {Email}", model.Email);

            // Gọi service để đăng nhập
            var (success, message, token) = await _authService.LoginAsync(model);

            if (!success)
            {
                _logger.LogWarning("Login failed for user {Email}: {Message}", model.Email, message);
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            _logger.LogInformation("User {Email} logged in successfully", model.Email);

            // Lưu token vào session
            HttpContext.Session.SetString("AccessToken", token);

            // Lấy thông tin profile người dùng
            var userProfile = await _authService.GetUserProfileAsync(token);

            // Tạo claims identity từ token
            var claimsPrincipal = await _authService.ValidateTokenAsync(token);
            if (claimsPrincipal == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to validate token");
                return View(model);
            }

            // Tạo danh sách claims mới từ claims hiện có
            var claims = claimsPrincipal.Claims.ToList();

            // Thêm claims cho first name và last name từ profile
            if (userProfile != null)
            {
                // Xóa claim name cũ nếu có
                var existingNameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                if (existingNameClaim != null)
                {
                    claims.Remove(existingNameClaim);
                }

                // Thêm claims mới
                claims.Add(new Claim(ClaimTypes.Name, userProfile.Email));
                claims.Add(new Claim("FirstName", userProfile.FirstName ?? ""));
                claims.Add(new Claim("LastName", userProfile.LastName ?? ""));
            }

            // Tạo identity và principal mới
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Kiểm tra vai trò để chuyển hướng admin
            var roles = await _authService.GetUserRolesAsync(token);
            if (roles != null && roles.Contains("Admin"))
            {
                _logger.LogInformation("Admin user {Email} redirected to admin app", model.Email);
                return Redirect("https://localhost:5002");
            }

            // Đăng nhập người dùng
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                });

            _logger.LogInformation("User {Email} authentication completed and redirected", model.Email);
            return LocalRedirect(returnUrl);
        }

        // GET: /Auth/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Kiểm tra tính hợp lệ của model
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _logger.LogInformation("Attempting to register user: {Email}", model.Email);

            // Gọi service để đăng ký
            var (success, message) = await _authService.RegisterAsync(model);

            if (success)
            {
                _logger.LogInformation("User {Email} registered successfully", model.Email);
                TempData["SuccessMessage"] = "Registration successful! You can now login.";
                return RedirectToAction(nameof(Login));
            }

            _logger.LogWarning("Registration failed for user {Email}: {Message}", model.Email, message);
            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        // GET: /Auth/Logout
        public async Task<IActionResult> Logout()
        {
            // Xóa session
            HttpContext.Session.Clear();

            // Đăng xuất khỏi cookie xác thực
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }
    }
}
