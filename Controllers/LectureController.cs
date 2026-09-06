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

        // GET: /Lecture
        [HttpGet]
        public async Task<IActionResult> Index(string? classLevel, string? subject, string? topic)
        {
            var query = _context.Lectures.AsQueryable();

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
        public async Task<IActionResult> Details(int id)
        {
            var lecture = await _context.Lectures.FirstOrDefaultAsync(l => l.Id == id);
            if (lecture == null)
            {
                return NotFound();
            }

            return View(lecture);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View(new LectureUploadViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(LectureUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? videoUrl = null;
            string? thumbnailUrl = null;

            // 1. If a new video file is uploaded: upload to Cloudinary (ONLY ONCE)
            if (model.VideoFile != null && model.VideoFile.Length > 0)
            {
                var uploadResult = await _cloudinaryService.UploadLectureVideoAsync(model.VideoFile);

                if (!uploadResult.Success)
                {
                    ModelState.AddModelError(nameof(model.VideoFile), uploadResult.ErrorMessage ?? "Failed to upload video to Cloudinary.");
                    return View(model);
                }

                videoUrl = uploadResult.SecureUrl;
                thumbnailUrl = uploadResult.ThumbnailUrl;
            }
            else if (!string.IsNullOrWhiteSpace(model.ExistingVideoUrl))
            {
                videoUrl = model.ExistingVideoUrl.Trim();
                thumbnailUrl = !string.IsNullOrWhiteSpace(model.ExistingThumbnailUrl)
                    ? model.ExistingThumbnailUrl.Trim()
                    : _cloudinaryService.GetVideoThumbnailUrl(videoUrl, 300, 200);
            }
            else
            {
                ModelState.AddModelError(nameof(model.VideoFile), "Please select a video file to upload or provide an existing Cloudinary URL.");
                return View(model);
            }

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
                Thumbnail = thumbnailUrl,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                __v = 0
            };

            _context.Lectures.Add(lecture);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Lecture uploaded successfully with status 'pending'!";
            return RedirectToAction(nameof(Index));
        }

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

        [HttpGet]
        public async Task<IActionResult> GetTopicsBySubject(string? classLevel, string? subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return Json(Array.Empty<string>());
            }

            var query = _context.Lectures.Where(l => l.Subject == subject);

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