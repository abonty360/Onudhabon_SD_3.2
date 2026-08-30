using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon_ISD.Data;
using Onudhabon_ISD.Models;
using Onudhabon_ISD.Services;

namespace Onudhabon_ISD.Controllers
{
    public class LectureController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<LectureController> _logger;

        public LectureController(
            ApplicationDbContext context,
            ICloudinaryService cloudinaryService,
            ILogger<LectureController> logger)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        private async Task<List<string>> GetCurrentUserIdentifiersAsync()
        {
            var identifiers = new List<string>();
            var userName = User.Identity?.Name;
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(userName)) identifiers.Add(userName.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(userEmail)) identifiers.Add(userEmail.Trim().ToLower());

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

        // GET: /Lecture
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? classLevel, string? subject, string? topic)
        {
            // Automatically discover and sync any existing Cloudinary assets if present
            try
            {
                var cloudinaryVideos = await _cloudinaryService.FetchCloudinaryLecturesAsync();
                if (cloudinaryVideos.Any())
                {
                    var existingUrls = await _context.Lectures.Select(l => l.VideoUrl).ToListAsync();
                    var newLectures = new List<Lecture>();

                    foreach (var cVid in cloudinaryVideos)
                    {
                        if (!string.IsNullOrEmpty(cVid.SecureUrl) && !existingUrls.Contains(cVid.SecureUrl))
                        {
                            newLectures.Add(new Lecture
                            {
                                Title = cVid.DisplayTitle,
                                Description = $"Recorded lecture video for {cVid.DisplayTitle}",
                                Instructor = "Educator",
                                Version = "Bangla",
                                ClassLevel = "General",
                                Subject = "General",
                                Topic = cVid.DisplayTitle,
                                VideoUrl = cVid.SecureUrl,
                                Thumbnail = _cloudinaryService.GetVideoThumbnailUrl(cVid.SecureUrl, 480, 270),
                                Status = "Active",
                                CreatedAt = cVid.CreatedAt,
                                __v = 0
                            });
                        }
                    }

                    if (newLectures.Any())
                    {
                        _context.Lectures.AddRange(newLectures);
                        await _context.SaveChangesAsync();
                    }
                }

                // Automatically ensure all existing lectures have Cloudinary thumbnails generated
                var lecturesNeedingThumbnails = await _context.Lectures
                    .Where(l => !string.IsNullOrEmpty(l.VideoUrl) && string.IsNullOrEmpty(l.Thumbnail))
                    .ToListAsync();

                if (lecturesNeedingThumbnails.Any())
                {
                    foreach (var lec in lecturesNeedingThumbnails)
                    {
                        lec.Thumbnail = _cloudinaryService.GetVideoThumbnailUrl(lec.VideoUrl, 480, 270);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Cloudinary video discovery skipped: {Message}", ex.Message);
            }

            var isAdmin = User.IsInRole("Admin");
            var query = _context.Lectures.AsQueryable();

            if (isAdmin)
            {
                // Admins see all lectures
            }
            else if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Logged-in educators/users see all approved lectures PLUS their own uploaded pending/declined lectures
                var userIdentifiers = await GetCurrentUserIdentifiersAsync();
                query = query.Where(l => l.Status == "Active" || l.Status == "Approved" || l.Status == "approved" 
                    || (l.Instructor != null && userIdentifiers.Contains(l.Instructor.ToLower())));
            }
            else
            {
                // Anonymous visitors only see approved lectures
                query = query.Where(l => l.Status == "Active" || l.Status == "Approved" || l.Status == "approved");
            }

            if (!string.IsNullOrWhiteSpace(classLevel))
            {
                query = query.Where(l => l.ClassLevel == classLevel);
            }

            if (!string.IsNullOrWhiteSpace(subject))
            {
                query = query.Where(l => l.Subject == subject);
            }

            if (!string.IsNullOrWhiteSpace(topic))
            {
                query = query.Where(l => l.Topic == topic);
            }

            var lectures = await query
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            ViewBag.ClassLevel = classLevel;
            ViewBag.Subject = subject;
            ViewBag.Topic = topic;

            return View(lectures);
        }

        // GET: /Lecture/Details/5
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var lecture = await _context.Lectures.FirstOrDefaultAsync(l => l.Id == id);
            if (lecture == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin");
            bool isApproved = lecture.Status == "Active" || lecture.Status == "Approved" || lecture.Status == "approved";
            bool isOwner = false;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdentifiers = await GetCurrentUserIdentifiersAsync();
                isOwner = !string.IsNullOrEmpty(lecture.Instructor) && userIdentifiers.Contains(lecture.Instructor.Trim().ToLower());
            }

            if (!isApproved && !isAdmin && !isOwner)
            {
                return NotFound();
            }

            return View(lecture);
        }

        // GET: /Lecture/Upload
        [HttpGet]
        [Authorize(Roles = "Educator")]
        public IActionResult Upload()
        {
            return View(new LectureUploadViewModel
            {
                Instructor = User.Identity?.Name
            });
        }

        // POST: /Lecture/Upload
        [HttpPost]
        [Authorize(Roles = "Educator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(LectureUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.VideoFile == null || model.VideoFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.VideoFile), "Please select a video file to upload.");
                return View(model);
            }

            var uploadResult = await _cloudinaryService.UploadLectureVideoAsync(model.VideoFile);

            if (!uploadResult.Success)
            {
                ModelState.AddModelError(nameof(model.VideoFile), uploadResult.ErrorMessage ?? "Failed to upload video to Cloudinary.");
                return View(model);
            }

            string? videoUrl = uploadResult.SecureUrl;
            string? thumbnailUrl = uploadResult.ThumbnailUrl ?? (videoUrl != null ? _cloudinaryService.GetVideoThumbnailUrl(videoUrl, 480, 270) : null);

            var lecture = new Lecture
            {
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                Instructor = !string.IsNullOrWhiteSpace(model.Instructor)
                    ? model.Instructor.Trim()
                    : (User.Identity?.Name ?? "Educator"),
                Version = model.Version?.Trim() ?? "Bangla",
                ClassLevel = model.ClassLevel.Trim(),
                Subject = model.Subject.Trim(),
                Topic = model.Topic.Trim(),
                VideoUrl = videoUrl,
                Thumbnail = thumbnailUrl ?? _cloudinaryService.GetVideoThumbnailUrl(videoUrl, 480, 270),
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                __v = 0
            };

            _context.Lectures.Add(lecture);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Lecture uploaded successfully with status 'pending'!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Lecture/GetApproved
        [HttpGet]
        public async Task<IActionResult> GetApproved(string? classLevel, string? subject)
        {
            var query = _context.Lectures
                .Where(l => l.Status == "Active" || l.Status == "Approved" || l.Status == "approved");

            if (!string.IsNullOrWhiteSpace(classLevel))
            {
                query = query.Where(l => l.ClassLevel == classLevel);
            }

            if (!string.IsNullOrWhiteSpace(subject))
            {
                query = query.Where(l => l.Subject == subject);
            }

            var approvedLectures = await query
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new
                {
                    l.Id,
                    l.Title,
                    l.Description,
                    l.Instructor,
                    l.Version,
                    l.ClassLevel,
                    l.Subject,
                    l.Topic,
                    l.VideoUrl,
                    l.Thumbnail,
                    l.Status,
                    l.CreatedAt
                })
                .ToListAsync();

            return Json(approvedLectures);
        }

        // GET: /Lecture/GetTopicsBySubject
        [HttpGet]
        public async Task<IActionResult> GetTopicsBySubject(string? classLevel, string? subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return Json(Array.Empty<string>());
            }

            var isAdmin = User.IsInRole("Admin");
            var query = _context.Lectures.Where(l => l.Subject == subject);

            if (!isAdmin)
            {
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var userIdentifiers = await GetCurrentUserIdentifiersAsync();
                    query = query.Where(l => l.Status == "Active" || l.Status == "Approved" || l.Status == "approved" 
                        || (l.Instructor != null && userIdentifiers.Contains(l.Instructor.ToLower())));
                }
                else
                {
                    query = query.Where(l => l.Status == "Active" || l.Status == "Approved" || l.Status == "approved");
                }
            }

            if (!string.IsNullOrWhiteSpace(classLevel))
            {
                query = query.Where(l => l.ClassLevel == classLevel);
            }

            var topics = await query
                .Where(l => !string.IsNullOrEmpty(l.Topic))
                .Select(l => l.Topic!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return Json(topics);
        }
    }
}
