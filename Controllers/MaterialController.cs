using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon_ISD.Data;
using Onudhabon_ISD.Models;
using Onudhabon_ISD.Services;

namespace Onudhabon_ISD.Controllers
{
    public class MaterialController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<MaterialController> _logger;

        public MaterialController(
            ApplicationDbContext context,
            ICloudinaryService cloudinaryService,
            ILogger<MaterialController> logger)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? classLevel, string? subject, string? topic)
        {
            var query = _context.Materials.AsQueryable();

            if (!string.IsNullOrWhiteSpace(classLevel))
            {
                query = query.Where(m => m.ClassLevel == classLevel);
            }

            if (!string.IsNullOrWhiteSpace(subject))
            {
                query = query.Where(m => m.Subject == subject);
            }

            if (!string.IsNullOrWhiteSpace(topic))
            {
                query = query.Where(m => m.Topic == topic);
            }

            var materials = await query
                .OrderByDescending(m => m.Date)
                .ToListAsync();

            ViewBag.ClassLevel = classLevel;
            ViewBag.Subject = subject;
            ViewBag.Topic = topic;

            return View(materials);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View(new MaterialUploadViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(MaterialUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? fileUrl = null;
            string? size = null;

            if (model.MaterialFile != null && model.MaterialFile.Length > 0)
            {
                var uploadResult = await _cloudinaryService.UploadMaterialPdfAsync(model.MaterialFile);

                if (!uploadResult.Success)
                {
                    ModelState.AddModelError(nameof(model.MaterialFile), uploadResult.ErrorMessage ?? "Failed to upload document to Cloudinary.");
                    return View(model);
                }

                fileUrl = uploadResult.SecureUrl;
                size = uploadResult.FormattedSize; 
            }

            else if (!string.IsNullOrWhiteSpace(model.ExistingFileUrl))
            {
                fileUrl = model.ExistingFileUrl.Trim();
                size = !string.IsNullOrWhiteSpace(model.ExistingSize)
                    ? model.ExistingSize.Trim()
                    : "0.00 MB";
            }
            else
            {
                ModelState.AddModelError(nameof(model.MaterialFile), "Please select a material document (PDF/DOCX) or provide an existing Cloudinary URL.");
                return View(model);
            }

            var material = new Material
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
                FileUrl = fileUrl,
                Size = size,
                Status = "pending",
                Downloads = 0,
                Date = DateTime.UtcNow,
                __v = 0
            };

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Material uploaded successfully with status 'pending'!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == id);
            if (material == null || string.IsNullOrWhiteSpace(material.FileUrl))
            {
                return NotFound();
            }

            material.Downloads++;
            await _context.SaveChangesAsync();

            return Redirect(material.FileUrl);
        }

        [HttpGet]
        public async Task<IActionResult> GetApproved(string? classLevel, string? subject)
        {
            var query = _context.Materials
                .Where(m => m.Status == "Active" || m.Status == "Approved" || m.Status == "approved");

            if (!string.IsNullOrWhiteSpace(classLevel))
            {
                query = query.Where(m => m.ClassLevel == classLevel);
            }

            if (!string.IsNullOrWhiteSpace(subject))
            {
                query = query.Where(m => m.Subject == subject);
            }

            var approvedMaterials = await query
                .OrderByDescending(m => m.Date)
                .Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.Description,
                    m.Instructor,
                    m.Version,
                    m.ClassLevel,
                    m.Subject,
                    m.Topic,
                    m.FileUrl,
                    m.Size,
                    m.Downloads,
                    m.Status,
                    m.Date
                })
                .ToListAsync();

            return Json(approvedMaterials);
        }


        [HttpGet]
        public async Task<IActionResult> GetTopicsBySubject(string? classLevel, string? subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return Json(Array.Empty<string>());
            }

            var query = _context.Materials.Where(m => m.Subject == subject);

            if (!string.IsNullOrWhiteSpace(classLevel))
            {
                query = query.Where(m => m.ClassLevel == classLevel);
            }

            var topics = await query
                .Where(m => !string.IsNullOrEmpty(m.Topic))
                .Select(m => m.Topic!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return Json(topics);
        }
    }
}