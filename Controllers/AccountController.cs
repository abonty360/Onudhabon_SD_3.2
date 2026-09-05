using Microsoft.AspNetCore.Mvc;
using Onudhabon.Models;

namespace Onudhabon.Controllers
{
    public class AccountController : Controller
    {
        /// <summary>
        /// Delivers the view via Login() GET and prepares form state.
        /// </summary>
        [HttpGet]
        [Route("Login")]
        [Route("Account/Login")]
        public IActionResult Login(string? returnUrl = null, string? modal = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!string.IsNullOrEmpty(modal))
            {
                ViewBag.ModalMessage = modal;
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        /// <summary>
        /// Handles authentication POST and passes alerts/modals (ViewBag.ModalMessage, TempData["SuccessMessage"]).
        /// </summary>
        [HttpPost]
        [Route("Login")]
        [Route("Account/Login")]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Demo authentication logic
            // In a real application, replace this with ASP.NET Identity or user store verification
            if (!string.IsNullOrWhiteSpace(model.UsernameOrEmail) && !string.IsNullOrWhiteSpace(model.Password))
            {
                TempData["SuccessMessage"] = $"Welcome back, {model.UsernameOrEmail}!";
                
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ModalMessage = "Invalid credentials. Please verify your username/email and password and try again.";
            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            return View(model);
        }

        [HttpGet]
        [Route("Register")]
        [Route("Account/Register")]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [Route("Register")]
        [Route("Account/Register")]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TempData["SuccessMessage"] = "Account created successfully! Please sign in.";
            return RedirectToAction("Login", new { returnUrl });
        }

        [HttpPost]
        [HttpGet]
        [Route("Account/GuestLogin")]
        public IActionResult GuestLogin()
        {
            TempData["SuccessMessage"] = "Logged in as Guest Explorer.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("Account/ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }
    }
}
