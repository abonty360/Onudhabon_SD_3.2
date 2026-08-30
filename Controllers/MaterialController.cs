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

        // GET: /Material
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? classLevel, string? subject, string? topic)
        {
            // Automatically discover and sync any existing Cloudinary material assets if present
            try
            {
                var cloudinaryDocs = await _cloudinaryService.FetchCloudinaryMaterialsAsync();
                if (cloudinaryDocs.Any())
                {
                    var existingUrls = await _context.Materials.Select(m => m.FileUrl).ToListAsync();
                    var newMaterials = new List<Material>();

                    foreach (var cDoc in cloudinaryDocs)
                    {
                        if (!string.IsNullOrEmpty(cDoc.SecureUrl) && !existingUrls.Contains(cDoc.SecureUrl))
                        {
                            newMaterials.Add(new Material
                            {
                                Title = cDoc.DisplayTitle,
                                Description = $"Educational study material for {cDoc.DisplayTitle}",
                                Instructor = "Educator",
                                Version = "Bangla",
                                ClassLevel = "General",
                                Subject = "General",
                                Topic = cDoc.DisplayTitle,
                                FileUrl = cDoc.SecureUrl,
                                Size = cDoc.FormattedSize,
                                Status = "Active",
                                Downloads = 0,
                                Date = cDoc.CreatedAt,
                                __v = 0
                            });
                        }
                    }

                    if (newMaterials.Any())
                    {
                        _context.Materials.AddRange(newMaterials);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Cloudinary material discovery skipped: {Message}", ex.Message);
            }

            var isAdmin = User.IsInRole("Admin");
            var query = _context.Materials.AsQueryable();

            if (isAdmin)
            {
                // Admins see all materials
            }
            else if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Logged-in educators/users see all active materials plus their own uploads (including pending/declined)
                var userIdentifiers = await GetCurrentUserIdentifiersAsync();
                query = query.Where(m => m.Status == "Active" || m.Status == "Approved" || m.Status == "approved" 
                    || (m.Instructor != null && userIdentifiers.Contains(m.Instructor.ToLower())));
            }
            else
            {
                // Anonymous visitors only see approved materials
                query = query.Where(m => m.Status == "Active" || m.Status == "Approved" || m.Status == "approved");
            }

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

        // GET: /Material/Details/5
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin");
            bool isApproved = material.Status == "Active" || material.Status == "Approved" || material.Status == "approved";
            bool isOwner = false;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdentifiers = await GetCurrentUserIdentifiersAsync();
                isOwner = !string.IsNullOrEmpty(material.Instructor) && userIdentifiers.Contains(material.Instructor.Trim().ToLower());
            }

            if (!isApproved && !isAdmin && !isOwner)
            {
                return NotFound();
            }

            return View(material);
        }

        // GET: /Material/Upload
        [HttpGet]
        [Authorize(Roles = "Educator")]
        public IActionResult Upload()
        {
            return View(new MaterialUploadViewModel
            {
                Instructor = User.Identity?.Name
            });
        }

        // POST: /Material/Upload
        [HttpPost]
        [Authorize(Roles = "Educator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(MaterialUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? fileUrl = null;
            string? size = null;

            // 1. If a new material file is uploaded: upload to Cloudinary (ONLY ONCE)
            if (model.MaterialFile != null && model.MaterialFile.Length > 0)
            {
                var uploadResult = await _cloudinaryService.UploadMaterialPdfAsync(model.MaterialFile);

                if (!uploadResult.Success)
                {
                    ModelState.AddModelError(nameof(model.MaterialFile), uploadResult.ErrorMessage ?? "Failed to upload document to Cloudinary.");
                    return View(model);
                }

                fileUrl = uploadResult.SecureUrl;
                size = uploadResult.FormattedSize; // Size formatted in MB (2 decimal places)
            }
            // 2. Otherwise if an existing Cloudinary URL is provided: reuse directly without re-uploading
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

        // GET: /Material/Download/5
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == id);
            if (material == null || string.IsNullOrWhiteSpace(material.FileUrl))
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin");
            bool isApproved = material.Status == "Active" || material.Status == "Approved" || material.Status == "approved";
            bool isOwner = false;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdentifiers = await GetCurrentUserIdentifiersAsync();
                isOwner = !string.IsNullOrEmpty(material.Instructor) && userIdentifiers.Contains(material.Instructor.Trim().ToLower());
            }

            if (!isApproved && !isAdmin && !isOwner)
            {
                return NotFound();
            }

            // Increment download count
            material.Downloads++;
            await _context.SaveChangesAsync();

            return Redirect(material.FileUrl);
        }

        // GET: /Material/GetApproved
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

        // GET: /Material/GetTopicsBySubject
        [HttpGet]
        public async Task<IActionResult> GetTopicsBySubject(string? classLevel, string? subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return Json(Array.Empty<string>());
            }

            var isAdmin = User.IsInRole("Admin");
            var query = _context.Materials.Where(m => m.Subject == subject);

            if (!isAdmin)
            {
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var userIdentifiers = await GetCurrentUserIdentifiersAsync();
                    query = query.Where(m => m.Status == "Active" || m.Status == "Approved" || m.Status == "approved" 
                        || (m.Instructor != null && userIdentifiers.Contains(m.Instructor.ToLower())));
                }
                else
                {
                    query = query.Where(m => m.Status == "Active" || m.Status == "Approved" || m.Status == "approved");
                }
            }

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
