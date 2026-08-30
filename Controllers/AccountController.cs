using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon_ISD.Data;
using Onudhabon_ISD.Models;
using Onudhabon_ISD.Services;

namespace Onudhabon_ISD.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ICloudinaryService _cloudinaryService;

        public AccountController(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher,
            ICloudinaryService cloudinaryService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Login(string? returnUrl = null)
        {
            SetNoCacheHeaders();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(returnUrl);
                }
                return RedirectAuthenticatedUser();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            SetNoCacheHeaders();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectAuthenticatedUser();
            }

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

            // Check if user account has been restricted by Admin
            if (user.IsRestricted)
            {
                ViewBag.RestrictedModal = true;
                ViewBag.ModalTitle = "Account Restricted";
                ViewBag.ModalMessage = "Your account has been restricted by the administrator. You are currently blocked from logging in. Please contact the administrator or support if you need assistance.";
                ModelState.AddModelError(string.Empty, "Your account has been restricted. You are blocked from logging in.");
                return View(model);
            }

            // Check if volunteer verification status was declined by Admin
            if (string.Equals(user.VerificationStatus, "Declined", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.DeclinedModal = true;
                ViewBag.ModalTitle = "Verification Status Declined";
                ViewBag.ModalMessage = "Your volunteer verification status has been declined by the administrator. Please contact support or the administrator for further inquiries.";
                ModelState.AddModelError(string.Empty, "Your verification status has been declined. You cannot log in.");
                return View(model);
            }

            // Create Claims for authenticated session
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("PhoneNumber", user.PhoneNumber ?? ""),
                new Claim("City", user.City ?? ""),
                new Claim("Area", user.Area ?? "")
            };

            if (!string.IsNullOrEmpty(user.Picture))
            {
                claims.Add(new Claim("Picture", user.Picture));
            }

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

            // If user is Admin, direct to the Admin Dashboard
            if (user.Role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Register(string? returnUrl = null)
        {
            SetNoCacheHeaders();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectAuthenticatedUser();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            SetNoCacheHeaders();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectAuthenticatedUser();
            }

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

            // Process uploaded files with Cloudinary
            string? picturePath = null;
            if (model.PictureFile != null && model.PictureFile.Length > 0)
            {
                var picResult = await _cloudinaryService.UploadProfilePictureAsync(model.PictureFile);
                if (picResult.Success)
                {
                    picturePath = picResult.SecureUrl;
                }
            }

            string? certificatePath = null;
            if (model.CertificatePictureFile != null && model.CertificatePictureFile.Length > 0)
            {
                var certResult = await _cloudinaryService.UploadEducationDocAsync(model.CertificatePictureFile);
                if (certResult.Success)
                {
                    certificatePath = certResult.SecureUrl;
                }
            }

            // Create new User entity with all submitted registration information
            var user = new User
            {
                FullName = model.FullName.Trim(),
                Email = emailNormalized,
                PhoneNumber = model.PhoneNumber.Trim(),
                Role = model.Role.Trim(),
                City = model.City.Trim(),
                Area = model.Area.Trim(),
                Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location.Trim(),
                Age = model.Age,
                NidNumber = string.IsNullOrWhiteSpace(model.NidNumber) ? null : model.NidNumber.Trim(),
                Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim(),
                Picture = picturePath,
                VolunteerReason = string.IsNullOrWhiteSpace(model.VolunteerReason) ? null : model.VolunteerReason.Trim(),
                EducationLevel = string.IsNullOrWhiteSpace(model.EducationLevel) ? null : model.EducationLevel.Trim(),
                Institution = !string.IsNullOrWhiteSpace(model.UniversityName) ? model.UniversityName.Trim() : (!string.IsNullOrWhiteSpace(model.HscInstitute) ? model.HscInstitute.Trim() : model.SscInstitute?.Trim()),
                Major = string.IsNullOrWhiteSpace(model.Major) ? null : model.Major.Trim(),
                CurrentlyStudying = string.IsNullOrWhiteSpace(model.CurrentlyStudying) ? null : model.CurrentlyStudying.Trim(),
                SscPassingYear = string.IsNullOrWhiteSpace(model.SscPassingYear) ? null : model.SscPassingYear.Trim(),
                SscInstitute = string.IsNullOrWhiteSpace(model.SscInstitute) ? null : model.SscInstitute.Trim(),
                HscPassingYear = string.IsNullOrWhiteSpace(model.HscPassingYear) ? null : model.HscPassingYear.Trim(),
                HscInstitute = string.IsNullOrWhiteSpace(model.HscInstitute) ? null : model.HscInstitute.Trim(),
                UniversityName = string.IsNullOrWhiteSpace(model.UniversityName) ? null : model.UniversityName.Trim(),
                UniversityPassingYear = string.IsNullOrWhiteSpace(model.UniversityPassingYear) ? null : model.UniversityPassingYear.Trim(),
                CertificatePicture = certificatePath,
                AgreeToTerms = model.AgreeToTerms,
                IsRestricted = false,
                IsVerified = false,
                VerificationStatus = "Pending",
                CreatedAt = DateTime.UtcNow,
                __v = 0
            };

            // Securely hash password
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Account registered successfully! Please sign in with your credentials.";
            return RedirectToAction("Login", new { returnUrl });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);

            User? user = null;
            if (int.TryParse(userIdStr, out int userId))
            {
                user = await _context.Users.FindAsync(userId);
            }

            if (user == null && !string.IsNullOrEmpty(email))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            }

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(user);
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

        private IActionResult RedirectAuthenticatedUser()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) || User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            if (string.Equals(role, "Local Guardian", StringComparison.OrdinalIgnoreCase) || User.IsInRole("Local Guardian"))
            {
                return RedirectToAction("Index", "Student");
            }
            if (string.Equals(role, "Educator", StringComparison.OrdinalIgnoreCase) || User.IsInRole("Educator"))
            {
                return RedirectToAction("Index", "Lecture");
            }
            return RedirectToAction("Index", "Home");
        }

        private void SetNoCacheHeaders()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "-1";
        }
    }
}
