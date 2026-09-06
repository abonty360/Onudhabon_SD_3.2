using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon.Models;
using Onudhabon_ISD.Data;
using Onudhabon_ISD.Models;

namespace Onudhabon_ISD.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var input = model.UsernameOrEmail?.Trim() ?? string.Empty;

            // Find user by Email (case-insensitive)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == input.ToLower());

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials. Please check your email and password.");
                return View(model);
            }

            // Verify password hash
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials. Please check your password.");
                return View(model);
            }

            // Create Claims for authenticated session
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("PhoneNumber", user.PhoneNumber),
                new Claim("City", user.City),
                new Claim("Area", user.Area)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddMinutes(30),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Restrict roles strictly to Educator or Local Guardian
            if (!string.IsNullOrEmpty(model.Role) && model.Role != "Educator" && model.Role != "Local Guardian")
            {
                ModelState.AddModelError(nameof(model.Role), "Please select a valid role: Educator or Local Guardian.");
            }

            if (!model.AgreeToTerms)
            {
                ModelState.AddModelError(nameof(model.AgreeToTerms), "You must agree to the Terms and Conditions to register.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var emailNormalized = model.Email.Trim().ToLower();

            // Check if user with this email already exists
            var existingUser = await _context.Users.AnyAsync(u => u.Email.ToLower() == emailNormalized);
            if (existingUser)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email address already exists.");
                return View(model);
            }

            // Create new User entity with exact informations from the register page
            var user = new User
            {
                FullName = model.FullName.Trim(),
                Email = emailNormalized,
                PhoneNumber = model.PhoneNumber.Trim(),
                Role = model.Role.Trim(),
                City = model.City.Trim(),
                Area = model.Area.Trim(),
                VolunteerReason = string.IsNullOrWhiteSpace(model.VolunteerReason) ? null : model.VolunteerReason.Trim(),
                AgreeToTerms = model.AgreeToTerms,
                CreatedAt = DateTime.UtcNow
            };

            // Securely hash password
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Account registered successfully! Please sign in with your credentials.";
            return RedirectToAction("Login", new { returnUrl });
        }

        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}