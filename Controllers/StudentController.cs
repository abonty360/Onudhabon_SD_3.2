using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon_ISD.Data;
using Onudhabon_ISD.Models;
using Onudhabon_ISD.Services;

namespace Onudhabon_ISD.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            ApplicationDbContext context,
            ICloudinaryService cloudinaryService,
            ILogger<StudentController> logger)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        private async Task<List<string>> GetCurrentUserIdentifiersAsync()
        {
            var identifiers = new List<string>();
            var userName = User.Identity?.Name;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(userName)) identifiers.Add(userName.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(userEmail)) identifiers.Add(userEmail.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(userIdClaim)) identifiers.Add(userIdClaim.Trim().ToLower());

            if (int.TryParse(userIdClaim, out int uid))
            {
                var dbUser = await _context.Users.FindAsync(uid);
                if (dbUser != null)
                {
                    if (!string.IsNullOrWhiteSpace(dbUser.FullName)) identifiers.Add(dbUser.FullName.Trim().ToLower());
                    if (!string.IsNullOrWhiteSpace(dbUser.Email)) identifiers.Add(dbUser.Email.Trim().ToLower());
                }
            }

            return identifiers.Distinct().ToList();
        }

        // GET: /Student or /Student/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole("Admin");
            var isLocalGuardian = User.IsInRole("Local Guardian") || User.FindFirst(ClaimTypes.Role)?.Value == "Local Guardian";

            if (isAdmin)
            {
                return RedirectToAction("Dashboard", "Admin", new { tab = "students" });
            }

            if (!isLocalGuardian)
            {
                TempData["ErrorMessage"] = "Student enrollment and guardianship management is available exclusively to registered Local Guardians.";
                return RedirectToAction("Index", "Home");
            }

            var identifiers = await GetCurrentUserIdentifiersAsync();

            var students = await _context.Students
                .Where(s => (s.GuardianId != null && identifiers.Contains(s.GuardianId.ToLower())) ||
                            (s.GuardianName != null && identifiers.Contains(s.GuardianName.ToLower())))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(students);
        }

        // GET: /Student/Enroll
        [HttpGet]
        [Authorize(Roles = "Local Guardian")]
        public IActionResult Enroll()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.Identity?.Name ?? "Local Guardian";

            var model = new StudentEnrollmentViewModel
            {
                GuardianName = userName,
                GuardianId = userIdClaim,
                EnrollmentYear = DateTime.UtcNow.Year.ToString()
            };

            return View(model);
        }

        private async Task<List<string>> GetSubjectsForClassLevelAsync(string? classLevel)
        {
            if (string.IsNullOrWhiteSpace(classLevel))
                return new List<string>();

            var cleanLevel = classLevel.Replace("Class", "", StringComparison.OrdinalIgnoreCase).Trim();
            var plan = await _context.ClassPlans
                .FirstOrDefaultAsync(p => p.ClassLevel == cleanLevel || p.ClassLevel == classLevel);

            if (plan?.Subjects != null && plan.Subjects.Any())
            {
                var validSubjects = plan.Subjects
                    .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                    .Select(s => s.Name.Trim())
                    .ToList();

                if (validSubjects.Any())
                    return validSubjects;
            }

            // Standard Bangladesh NCTB curriculum fallback by class level
            if (int.TryParse(cleanLevel, out int lvl))
            {
                if (lvl >= 1 && lvl <= 3)
                    return new List<string> { "Bangla", "English", "Math" };
                if (lvl >= 4 && lvl <= 8)
                    return new List<string> { "Bangla 1st paper", "Bangla 2nd paper", "English 1st paper", "English 2nd paper", "Math", "Social Science", "General Science" };
                if (lvl >= 9 && lvl <= 10)
                    return new List<string> { "Bangla 1st paper", "Bangla 2nd paper", "English 1st paper", "English 2nd paper", "Math", "Social Science", "General Science", "Physics", "Chemistry", "Higher Math", "Biology" };
                if (lvl >= 11 && lvl <= 12)
                    return new List<string> { "Bangla 1st paper", "Bangla 2nd paper", "English 1st paper", "English 2nd paper", "Physics 1st paper", "Physics 2nd paper", "Chemistry 1st paper", "Chemistry 2nd paper", "Higher Math 1st paper", "Higher Math 2nd paper", "Biology 1st paper", "Biology 2nd paper" };
            }

            return new List<string>();
        }

        // GET: /Student/GetClassPlanSubjects?classLevel=5
        [HttpGet]
        public async Task<IActionResult> GetClassPlanSubjects(string? classLevel)
        {
            if (string.IsNullOrWhiteSpace(classLevel))
            {
                return Json(new { success = true, subjects = new List<string>(), formatted = "" });
            }

            var subjectNames = await GetSubjectsForClassLevelAsync(classLevel);
            var formatted = string.Join(", ", subjectNames);

            return Json(new { success = true, subjects = subjectNames, formatted });
        }

        // GET: /Student/CheckBirthCertificateId?birthCertificateId=...
        [HttpGet]
        public async Task<IActionResult> CheckBirthCertificateId(string? birthCertificateId)
        {
            if (string.IsNullOrWhiteSpace(birthCertificateId))
            {
                return Json(new { isUnique = true });
            }

            var cleanId = birthCertificateId.Trim();
            bool exists = await _context.Students.AnyAsync(s =>
                s.BirthCertificateId != null && s.BirthCertificateId.ToLower() == cleanId.ToLower());

            return Json(new { isUnique = !exists });
        }

        // POST: /Student/Enroll
        [HttpPost]
        [Authorize(Roles = "Local Guardian")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(StudentEnrollmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Enforce Unique Birth Certificate ID
            if (!string.IsNullOrWhiteSpace(model.BirthCertificateId))
            {
                var cleanBirthCertId = model.BirthCertificateId.Trim();
                bool exists = await _context.Students.AnyAsync(s =>
                    s.BirthCertificateId != null && s.BirthCertificateId.ToLower() == cleanBirthCertId.ToLower());

                if (exists)
                {
                    ModelState.AddModelError(nameof(model.BirthCertificateId), "A student with this Birth Certificate ID is already enrolled in the platform. The Birth Certificate ID must be unique.");
                    return View(model);
                }
            }

            // Auto-generate Enrolled Subjects strictly from Class Plan in database
            var subjectsList = await GetSubjectsForClassLevelAsync(model.ClassLevel);
            string autoGeneratedSubjects = subjectsList.Any()
                ? string.Join(", ", subjectsList)
                : (!string.IsNullOrWhiteSpace(model.Subjects) ? model.Subjects.Trim() : "General Curriculum");

            string? consentLetterUrl = null;

            // Upload consent letter file if provided
            if (model.ConsentLetterFile != null && model.ConsentLetterFile.Length > 0)
            {
                var uploadResult = await _cloudinaryService.UploadConsentLetterAsync(model.ConsentLetterFile);
                if (!uploadResult.Success)
                {
                    ModelState.AddModelError(nameof(model.ConsentLetterFile), uploadResult.ErrorMessage ?? "Failed to upload consent letter to Cloudinary.");
                    return View(model);
                }

                consentLetterUrl = uploadResult.SecureUrl;
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.Identity?.Name ?? "Local Guardian";

            var student = new Student
            {
                FullName = model.FullName.Trim(),
                BirthCertificateId = string.IsNullOrWhiteSpace(model.BirthCertificateId) ? null : model.BirthCertificateId.Trim(),
                Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim(),
                FatherName = string.IsNullOrWhiteSpace(model.FatherName) ? null : model.FatherName.Trim(),
                MotherName = string.IsNullOrWhiteSpace(model.MotherName) ? null : model.MotherName.Trim(),
                ClassLevel = model.ClassLevel.Trim(),
                Subjects = autoGeneratedSubjects,
                EnrollmentYear = string.IsNullOrWhiteSpace(model.EnrollmentYear) ? DateTime.UtcNow.Year.ToString() : model.EnrollmentYear.Trim(),
                ConsentLetterUrl = consentLetterUrl,
                GuardianId = !string.IsNullOrWhiteSpace(model.GuardianId) ? model.GuardianId.Trim() : userIdClaim,
                GuardianName = !string.IsNullOrWhiteSpace(model.GuardianName) ? model.GuardianName.Trim() : userName,
                Status = "Pending",
                CompletedClasses = 0,
                CreatedAt = DateTime.UtcNow,
                __v = 0
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Student '{student.FullName}' has been enrolled successfully with status 'Pending'. Awaiting administrator verification.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Student/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin");
            var identifiers = await GetCurrentUserIdentifiersAsync();
            bool isOwner = (!string.IsNullOrEmpty(student.GuardianId) && identifiers.Contains(student.GuardianId.ToLower())) ||
                           (!string.IsNullOrEmpty(student.GuardianName) && identifiers.Contains(student.GuardianName.ToLower()));

            if (!isAdmin && !isOwner)
            {
                return NotFound();
            }

            return View(student);
        }
    }
}
