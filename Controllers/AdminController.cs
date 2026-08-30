using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon_ISD.Data;
using Onudhabon_ISD.Models;
using Onudhabon_ISD.Services;

namespace Onudhabon_ISD.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            ICloudinaryService cloudinaryService,
            ILogger<AdminController> logger)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        // GET: /Admin or /Admin/Dashboard
        [HttpGet]
        public async Task<IActionResult> Index() => await Dashboard();

        [HttpGet]
        public async Task<IActionResult> Dashboard(string? tab = "volunteers")
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var lectures = await _context.Lectures
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            var materials = await _context.Materials
                .OrderByDescending(m => m.Date)
                .ToListAsync();

            var forumPosts = await _context.ForumPosts
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var students = await _context.Students
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var donations = await _context.Donations
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                Users = users,
                Lectures = lectures,
                Materials = materials,
                ForumPosts = forumPosts,
                Students = students,
                Donations = donations
            };

            ViewBag.ActiveTab = tab ?? "volunteers";
            return View(viewModel);
        }

        // POST: /Admin/ApproveUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.VerificationStatus = "Active";
            user.IsVerified = true;
            user.IsRestricted = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Volunteer '{user.FullName}' ({user.Role}) has been approved. Status is now Active.";
            return RedirectToAction(nameof(Dashboard), new { tab = "volunteers" });
        }

        // POST: /Admin/DeclineUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.VerificationStatus = "Declined";
            user.IsVerified = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Volunteer '{user.FullName}' registration has been declined.";
            return RedirectToAction(nameof(Dashboard), new { tab = "volunteers" });
        }

        // POST: /Admin/ToggleRestrictUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRestrictUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsRestricted = !user.IsRestricted;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"User '{user.FullName}' restriction updated. Account is now {(user.IsRestricted ? "RESTRICTED (Blocked from login)" : "UNRESTRICTED (Active)")}.";
            return RedirectToAction(nameof(Dashboard), new { tab = "volunteers" });
        }

        // POST: /Admin/ApproveLecture/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLecture(int id)
        {
            var lecture = await _context.Lectures.FindAsync(id);
            if (lecture == null) return NotFound();

            lecture.Status = "Active";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Lecture '{lecture.Title}' has been approved and is now live for all learners.";
            return RedirectToAction(nameof(Dashboard), new { tab = "lectures" });
        }

        // POST: /Admin/DeclineLecture/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineLecture(int id)
        {
            var lecture = await _context.Lectures.FindAsync(id);
            if (lecture == null) return NotFound();

            lecture.Status = "Declined";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Lecture '{lecture.Title}' has been declined.";
            return RedirectToAction(nameof(Dashboard), new { tab = "lectures" });
        }

        // POST: /Admin/ApproveMaterial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveMaterial(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return NotFound();

            material.Status = "Active";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Material '{material.Title}' has been approved and is available for download.";
            return RedirectToAction(nameof(Dashboard), new { tab = "materials" });
        }

        // POST: /Admin/DeclineMaterial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineMaterial(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return NotFound();

            material.Status = "Declined";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Material '{material.Title}' has been declined.";
            return RedirectToAction(nameof(Dashboard), new { tab = "materials" });
        }

        // POST: /Admin/ApproveForumPost/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveForumPost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();

            post.Status = "Active";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Forum post '{post.Title}' has been approved and is now live in the community forum.";
            return RedirectToAction(nameof(Dashboard), new { tab = "forum" });
        }

        // POST: /Admin/DeclineForumPost/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineForumPost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();

            post.Status = "Declined";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Forum post '{post.Title}' has been declined.";
            return RedirectToAction(nameof(Dashboard), new { tab = "forum" });
        }

        // POST: /Admin/ApproveStudent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            student.Status = "Active";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Student '{student.FullName}' enrollment has been approved.";
            return RedirectToAction(nameof(Dashboard), new { tab = "students" });
        }

        // POST: /Admin/DeclineStudent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            student.Status = "Declined";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Student '{student.FullName}' enrollment has been declined.";
            return RedirectToAction(nameof(Dashboard), new { tab = "students" });
        }

        // POST: /Admin/RestrictEducator or RestrictUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestrictEducator(string educatorName, string? returnTab = "lectures")
        {
            if (string.IsNullOrWhiteSpace(educatorName))
            {
                TempData["ErrorMessage"] = "User identifier is missing.";
                return RedirectToAction(nameof(Dashboard), new { tab = returnTab });
            }

            var cleanName = educatorName.Trim().ToLower();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.FullName.ToLower() == cleanName || u.Email.ToLower() == cleanName);

            if (user == null)
            {
                TempData["ErrorMessage"] = $"No registered user matching '{educatorName}' was found in the database.";
                return RedirectToAction(nameof(Dashboard), new { tab = returnTab });
            }

            user.IsRestricted = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"User '{user.FullName}' ({user.Email}) has been restricted and blocked from logging in.";
            return RedirectToAction(nameof(Dashboard), new { tab = returnTab });
        }
    }
}
