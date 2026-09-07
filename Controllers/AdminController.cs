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
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var studentCount = await _context.Students.CountAsync();

            var viewModel = new AdminDashboardViewModel
            {
                Users = users,
                Lectures = lectures,
                Materials = materials,
                ForumPosts = forumPosts,
                TotalStudentsCount = studentCount
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

            var wasNotActive = user.VerificationStatus != "Active";

            user.VerificationStatus = "Active";
            user.IsVerified = true;
            user.IsRestricted = false;
            await _context.SaveChangesAsync();

            // Send notification to user if status changed to Active
            if (wasNotActive)
            {
                var notification = new Notification
                {
                    User = user.Email,
                    Sender = "System Admin",
                    Post = $"Your {user.Role} volunteer account has been approved and activated.",
                    Type = "VolunteerApproval",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    __v = 0
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

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

            var wasNotActive = lecture.Status != "Active";

            lecture.Status = "Active";
            await _context.SaveChangesAsync();

            // Send notification to instructor if status changed to Active
            if (wasNotActive && !string.IsNullOrWhiteSpace(lecture.Instructor))
            {
                var notification = new Notification
                {
                    User = lecture.Instructor,
                    Sender = "System Admin",
                    Post = $"Your recorded lecture \"{lecture.Title}\" has been approved and published.",
                    Type = "LectureApproval",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    __v = 0
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

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

            var wasNotActive = material.Status != "Active";

            material.Status = "Active";
            await _context.SaveChangesAsync();

            // Send notification to instructor if status changed to Active
            if (wasNotActive && !string.IsNullOrWhiteSpace(material.Instructor))
            {
                var notification = new Notification
                {
                    User = material.Instructor,
                    Sender = "System Admin",
                    Post = $"Your study material \"{material.Title}\" has been approved and is now available for download.",
                    Type = "MaterialApproval",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    __v = 0
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

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

        // POST: /Admin/RestrictEducator
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestrictEducator(string educatorName, string? returnTab = "lectures")
        {
            if (string.IsNullOrWhiteSpace(educatorName))
            {
                TempData["ErrorMessage"] = "Educator identifier is missing.";
                return RedirectToAction(nameof(Dashboard), new { tab = returnTab });
            }

            var cleanName = educatorName.Trim().ToLower();
            var educator = await _context.Users
                .FirstOrDefaultAsync(u => u.FullName.ToLower() == cleanName || u.Email.ToLower() == cleanName);

            if (educator == null)
            {
                TempData["ErrorMessage"] = $"No registered user matching educator '{educatorName}' was found in the database.";
                return RedirectToAction(nameof(Dashboard), new { tab = returnTab });
            }

            educator.IsRestricted = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Educator '{educator.FullName}' ({educator.Email}) has been restricted and blocked from logging in.";
            return RedirectToAction(nameof(Dashboard), new { tab = returnTab });
        }

        // POST: /Admin/ApproveForumPost/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveForumPost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();

            var wasNotActive = post.Status != "Active";

            post.Status = "Active";
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Send notification to author if status changed to Active
            if (wasNotActive && !string.IsNullOrWhiteSpace(post.Author))
            {
                var notification = new Notification
                {
                    User = post.Author,
                    Sender = "System Admin",
                    Post = $"Your forum post \"{post.Title}\" has been approved and is now active.",
                    Type = "ForumApproval",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    __v = 0
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Forum post '{post.Title}' has been approved and is now active.";
            return RedirectToAction(nameof(Dashboard), new { tab = "forum" });
        }

        // POST: /Admin/ApproveStudent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            var wasNotActive = student.Status != "Active";

            student.Status = "Active";
            await _context.SaveChangesAsync();

            // Send notification to guardian/submitter if status changed to Active
            var recipient = student.GuardianName ?? student.GuardianId;
            if (wasNotActive && !string.IsNullOrWhiteSpace(recipient))
            {
                var notification = new Notification
                {
                    User = recipient,
                    Sender = "System Admin",
                    Post = $"Your student enrollment submission for '{student.FullName}' (Class {student.ClassLevel}) has been approved.",
                    Type = "StudentApproval",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    __v = 0
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Student '{student.FullName}' has been approved.";
            return RedirectToAction(nameof(Dashboard), new { tab = "students" });
        }

        // POST: /Admin/DisapproveForumPost/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisapproveForumPost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();

            post.Status = "Declined";
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Forum post '{post.Title}' has been declined/disapproved.";
            return RedirectToAction(nameof(Dashboard), new { tab = "forum" });
        }

        // POST: /Admin/DeleteForumPost/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteForumPost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();

            _context.ForumPosts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Forum post '{post.Title}' has been deleted.";
            return RedirectToAction(nameof(Dashboard), new { tab = "forum" });
        }
    }
}