using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon.Data;
using Onudhabon.Models;
using System.Security.Claims;

namespace Onudhabon.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(ApplicationDbContext context, ILogger<NotificationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Notification/GetNotifications
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var identifiers = GetCurrentUserIdentifiers();
            if (!identifiers.Any())
            {
                return Json(new { success = true, unreadCount = 0, notifications = new List<object>() });
            }

            var notifications = await _context.Notifications
                .Where(n => n.User != null && identifiers.Contains(n.User.ToLower()))
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .ToListAsync();

            var unreadCount = await _context.Notifications
                .CountAsync(n => n.User != null && identifiers.Contains(n.User.ToLower()) && !n.IsRead);

            var items = notifications.Select(n => new
            {
                id = n.Id,
                sender = n.Sender ?? "System",
                message = FormatNotificationMessage(n),
                type = n.Type ?? "General",
                icon = GetNotificationIcon(n.Type),
                link = GetNotificationLink(n),
                isRead = n.IsRead,
                timeAgo = GetTimeAgo(n.CreatedAt),
                createdAt = n.CreatedAt.ToString("MMM dd, yyyy h:mm tt")
            }).ToList();

            return Json(new
            {
                success = true,
                unreadCount = unreadCount,
                notifications = items
            });
        }

        // POST: /Notification/MarkSingleNotificationAsRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkSingleNotificationAsRead(int id)
        {
            var identifiers = GetCurrentUserIdentifiers();
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return Json(new { success = false, message = "Notification not found." });
            }

            if (notification.User == null || !identifiers.Contains(notification.User.ToLower()))
            {
                return Json(new { success = false, message = "Unauthorized to update this notification." });
            }

            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var unreadCount = await _context.Notifications
                .CountAsync(n => n.User != null && identifiers.Contains(n.User.ToLower()) && !n.IsRead);

            return Json(new { success = true, unreadCount = unreadCount });
        }

        // POST: /Notification/MarkNotificationsAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationsAsRead()
        {
            var identifiers = GetCurrentUserIdentifiers();
            if (!identifiers.Any())
            {
                return Json(new { success = true, unreadCount = 0 });
            }

            var unreadNotifications = await _context.Notifications
                .Where(n => n.User != null && identifiers.Contains(n.User.ToLower()) && !n.IsRead)
                .ToListAsync();

            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
                n.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, unreadCount = 0 });
        }

        // GET: /Notification or /Notification/Index
        [HttpGet]
        public async Task<IActionResult> Index(string? filter = "all")
        {
            var identifiers = GetCurrentUserIdentifiers();

            IQueryable<Notification> query = _context.Notifications
                .Where(n => n.User != null && identifiers.Contains(n.User.ToLower()))
                .OrderByDescending(n => n.CreatedAt);

            if (filter == "unread")
            {
                query = query.Where(n => !n.IsRead);
            }

            var list = await query.ToListAsync();
            ViewBag.ActiveFilter = filter ?? "all";
            ViewBag.UnreadCount = await _context.Notifications
                .CountAsync(n => n.User != null && identifiers.Contains(n.User.ToLower()) && !n.IsRead);

            return View(list);
        }

        private List<string> GetCurrentUserIdentifiers()
        {
            var identifiers = new List<string>();

            var name = User.Identity?.Name?.Trim().ToLower();
            if (!string.IsNullOrEmpty(name)) identifiers.Add(name);

            var email = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLower();
            if (!string.IsNullOrEmpty(email) && !identifiers.Contains(email)) identifiers.Add(email);

            var id = User.FindFirstValue(ClaimTypes.NameIdentifier)?.Trim().ToLower();
            if (!string.IsNullOrEmpty(id) && !identifiers.Contains(id)) identifiers.Add(id);

            return identifiers;
        }

        private static string FormatNotificationMessage(Notification n)
        {
            var type = n.Type ?? string.Empty;
            return type switch
            {
                "LectureApproval" => string.IsNullOrWhiteSpace(n.Post)
                    ? "Your lecture has been approved and is now live."
                    : $"Your lecture \"{n.Post}\" has been approved and is now live.",

                "MaterialApproval" => string.IsNullOrWhiteSpace(n.Post)
                    ? "Your study material has been approved and published."
                    : $"Your material \"{n.Post}\" has been approved and published.",

                "StudentApproval" => string.IsNullOrWhiteSpace(n.Post)
                    ? "Your student enrollment submission has been approved."
                    : n.Post,

                "ForumApproval" => string.IsNullOrWhiteSpace(n.Post)
                    ? "Your forum discussion has been approved and is now active."
                    : $"Your forum post \"{n.Post}\" has been approved and is now active.",

                "VolunteerApproval" => string.IsNullOrWhiteSpace(n.Post)
                    ? "Your volunteer account has been approved and verified."
                    : n.Post,

                "Like" => $"{n.Sender ?? "Someone"} liked your post: \"{n.Post ?? "Discussion"}\"",
                "Dislike" => $"{n.Sender ?? "Someone"} reacted to your post: \"{n.Post ?? "Discussion"}\"",
                "Comment" => $"{n.Sender ?? "Someone"} commented on your post: \"{n.Post ?? "Discussion"}\"",

                _ => !string.IsNullOrWhiteSpace(n.Post) ? n.Post : "You have a new update."
            };
        }

        private static string GetNotificationIcon(string? type)
        {
            return type switch
            {
                "LectureApproval" => "🎥",
                "MaterialApproval" => "📄",
                "StudentApproval" => "🎓",
                "ForumApproval" => "💬",
                "VolunteerApproval" => "🛡️",
                "Like" => "👍",
                "Dislike" => "👎",
                "Comment" => "💬",
                _ => "🔔"
            };
        }

        private static string GetNotificationLink(Notification n)
        {
            var type = n.Type ?? string.Empty;
            return type switch
            {
                "LectureApproval" => "/Lecture",
                "MaterialApproval" => "/Material",
                "StudentApproval" => "/Admin/Dashboard?tab=students",
                "ForumApproval" or "Like" or "Dislike" or "Comment" => "/Forum",
                "VolunteerApproval" => "/Account/Profile",
                _ => "/Notification"
            };
        }

        private static string GetTimeAgo(DateTime dt)
        {
            var span = DateTime.UtcNow - dt;
            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
            return dt.ToString("MMM dd");
        }
    }
}
