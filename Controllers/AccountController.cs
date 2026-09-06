using Microsoft.AspNetCore.Mvc;
using Onudhabon.Models;

namespace Onudhabon.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Authentication validation placeholder
            if (string.IsNullOrWhiteSpace(model.UsernameOrEmail) || string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials.");
                return View(model);
            }

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
        public IActionResult Register(RegisterViewModel model, string? returnUrl = null)
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

            // Successful registration: redirect to Login page with success message
            TempData["SuccessMessage"] = "Account registered successfully! Please sign in.";
            return RedirectToAction("Login", new { returnUrl });
        }

        [HttpGet]
        public IActionResult Logout()
        {
            return RedirectToAction("Login", "Account");
        }
    }
}