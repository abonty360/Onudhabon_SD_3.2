using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon.Data;
using Onudhabon.Models;
using Onudhabon.Services;
using System.Security.Claims;

namespace Onudhabon.Controllers
{
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public ForumController(ApplicationDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        private int? GetCurrentUserId()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return null;
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            return null;
        }

        private async Task<User?> FindUserByAuthorAsync(string? authorName)
        {
            if (string.IsNullOrWhiteSpace(authorName)) return null;

            var clean = authorName.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.FullName.ToLower() == clean.ToLower() ||
                u.Email.ToLower() == clean.ToLower() ||
                (clean.Equals("System Admin", StringComparison.OrdinalIgnoreCase) && u.Role == "Admin"));

            if (user == null && int.TryParse(clean, out int uid))
            {
                user = await _context.Users.FindAsync(uid);
            }

            return user;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var userName = User.Identity?.Name;
            var isAdmin = User.IsInRole("Admin");

            var query = _context.ForumPosts.AsQueryable();

            if (isAdmin)
            {
                // Admins see all posts
            }
            else if (!string.IsNullOrEmpty(userName))
            {
                // Logged in users see all active posts plus their own posts (including Pending/Declined)
                query = query.Where(p => p.Status == "Active" || p.Status == "Approved" || p.Status == "approved" || p.Author == userName);
            }
            else
            {
                // Anonymous visitors only see approved posts
                query = query.Where(p => p.Status == "Active" || p.Status == "Approved" || p.Status == "approved");
            }

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var userReactions = new Dictionary<int, bool>(); // PostId -> IsLike (true = like, false = dislike)
            var currentUserId = GetCurrentUserId();
            if (currentUserId.HasValue)
            {
                var postIds = posts.Select(p => p.Id).ToList();
                var reactions = await _context.ForumPostReactions
                    .Where(r => r.UserId == currentUserId.Value && postIds.Contains(r.PostId))
                    .ToListAsync();

                userReactions = reactions.ToDictionary(r => r.PostId, r => r.IsLike);
            }

            // Build map of author profiles
            var authorNames = posts
                .Where(p => !string.IsNullOrEmpty(p.Author))
                .Select(p => p.Author!)
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(u => authorNames.Contains(u.FullName) || authorNames.Contains(u.Email) || (authorNames.Contains("System Admin") && u.Role == "Admin"))
                .ToListAsync();

            var authorProfiles = new Dictionary<string, ForumAuthorDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var aName in authorNames)
            {
                var matchingUser = users.FirstOrDefault(u =>
                    u.FullName.Equals(aName, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Equals(aName, StringComparison.OrdinalIgnoreCase) ||
                    (aName.Equals("System Admin", StringComparison.OrdinalIgnoreCase) && u.Role == "Admin"));

                if (matchingUser != null)
                {
                    authorProfiles[aName] = new ForumAuthorDto
                    {
                        FullName = matchingUser.FullName,
                        Role = matchingUser.Role,
                        AvatarUrl = !string.IsNullOrWhiteSpace(matchingUser.Picture)
                            ? _cloudinaryService.GetAvatarUrl(matchingUser.Picture, 90)
                            : null,
                        Initial = string.IsNullOrEmpty(matchingUser.FullName) ? "U" : matchingUser.FullName.Substring(0, 1).ToUpper(),
                        IsVerified = matchingUser.IsVerified || string.Equals(matchingUser.VerificationStatus, "Active", StringComparison.OrdinalIgnoreCase) || string.Equals(matchingUser.VerificationStatus, "Approved", StringComparison.OrdinalIgnoreCase),
                        VerificationStatus = matchingUser.VerificationStatus ?? "Pending",
                        Bio = matchingUser.Bio,
                        City = matchingUser.City,
                        Area = matchingUser.Area
                    };
                }
            }

            ViewBag.AuthorProfiles = authorProfiles;
            ViewBag.UserReactions = userReactions;
            return View(posts);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(string title, string content, string? category, string? tags)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Title and Content are required.";
                return RedirectToAction(nameof(Index));
            }

            var userName = User.Identity?.Name ?? "Anonymous";
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
            var isAdmin = User.IsInRole("Admin") || userRole == "Admin";

            var post = new ForumPost
            {
                Title = title.Trim(),
                Content = content.Trim(),
                Author = userName,
                AuthorRole = userRole,
                Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
                Tags = string.IsNullOrWhiteSpace(tags) ? "#discussion" : (tags.StartsWith("#") ? tags.Trim() : "#" + tags.Trim()),
                Status = isAdmin ? "Active" : "Pending",
                CreatedAt = DateTime.UtcNow,
                Likes = 0,
                Dislikes = 0,
                Replies = 0
            };

            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();

            if (post.Status == "Active")
            {
                TempData["SuccessMessage"] = "Forum post created and published successfully!";
            }
            else
            {
                TempData["SuccessMessage"] = "Your discussion post has been submitted for admin approval (Status: Pending).";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Json(new { 
                    success = false, 
                    requireLogin = true, 
                    redirectUrl = Url.Action("Login", "Account", new { returnUrl = "/Forum" }) 
                });
            }

            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null)
            {
                return Json(new { success = false, message = "Post not found." });
            }

            var userId = currentUserId.Value;
            var existingReaction = await _context.ForumPostReactions
                .FirstOrDefaultAsync(r => r.PostId == id && r.UserId == userId);

            string userReaction;

            if (existingReaction != null)
            {
                if (existingReaction.IsLike)
                {
                    // 2nd click on Like -> Undo Like
                    _context.ForumPostReactions.Remove(existingReaction);
                    post.Likes = Math.Max(0, post.Likes - 1);
                    userReaction = "none";
                }
                else
                {
                    // Switched from Dislike to Like
                    existingReaction.IsLike = true;
                    existingReaction.CreatedAt = DateTime.UtcNow;
                    post.Dislikes = Math.Max(0, post.Dislikes - 1);
                    post.Likes += 1;
                    userReaction = "like";
                }
            }
            else
            {
                // First reaction: Add Like
                _context.ForumPostReactions.Add(new ForumPostReaction
                {
                    PostId = id,
                    UserId = userId,
                    IsLike = true,
                    CreatedAt = DateTime.UtcNow
                });
                post.Likes += 1;

                var sender = User.Identity?.Name ?? "Someone";
                if (!string.IsNullOrEmpty(post.Author) && !post.Author.Equals(sender, StringComparison.OrdinalIgnoreCase))
                {
                    var notification = new Notification
                    {
                        User = post.Author,
                        Sender = sender,
                        Post = post.Title,
                        Type = "Like",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        __v = 0
                    };
                    _context.Notifications.Add(notification);
                }

                userReaction = "like";
            }

            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                likes = post.Likes, 
                dislikes = post.Dislikes,
                userReaction = userReaction 
            });
        }

        [HttpPost]
        public async Task<IActionResult> Dislike(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Json(new { 
                    success = false, 
                    requireLogin = true, 
                    redirectUrl = Url.Action("Login", "Account", new { returnUrl = "/Forum" }) 
                });
            }

            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null)
            {
                return Json(new { success = false, message = "Post not found." });
            }

            var userId = currentUserId.Value;
            var existingReaction = await _context.ForumPostReactions
                .FirstOrDefaultAsync(r => r.PostId == id && r.UserId == userId);

            string userReaction;

            if (existingReaction != null)
            {
                if (!existingReaction.IsLike)
                {
                    // 2nd click on Dislike -> Undo Dislike
                    _context.ForumPostReactions.Remove(existingReaction);
                    post.Dislikes = Math.Max(0, post.Dislikes - 1);
                    userReaction = "none";
                }
                else
                {
                    // Switched from Like to Dislike
                    existingReaction.IsLike = false;
                    existingReaction.CreatedAt = DateTime.UtcNow;
                    post.Likes = Math.Max(0, post.Likes - 1);
                    post.Dislikes += 1;
                    userReaction = "dislike";
                }
            }
            else
            {
                // First reaction: Add Dislike
                _context.ForumPostReactions.Add(new ForumPostReaction
                {
                    PostId = id,
                    UserId = userId,
                    IsLike = false,
                    CreatedAt = DateTime.UtcNow
                });
                post.Dislikes += 1;
                userReaction = "dislike";
            }

            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                likes = post.Likes, 
                dislikes = post.Dislikes,
                userReaction = userReaction 
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetComments(int postId)
        {
            var comments = await _context.ForumComments
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var authorNames = comments
                .Where(c => !string.IsNullOrEmpty(c.Author))
                .Select(c => c.Author!)
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(u => authorNames.Contains(u.FullName) || authorNames.Contains(u.Email) || (authorNames.Contains("System Admin") && u.Role == "Admin"))
                .ToListAsync();

            var result = comments.Select(c => {
                var user = users.FirstOrDefault(u =>
                    u.FullName.Equals(c.Author, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Equals(c.Author, StringComparison.OrdinalIgnoreCase) ||
                    (c.Author.Equals("System Admin", StringComparison.OrdinalIgnoreCase) && u.Role == "Admin"));

                var avatarUrl = (user != null && !string.IsNullOrWhiteSpace(user.Picture))
                    ? _cloudinaryService.GetAvatarUrl(user.Picture, 80)
                    : null;

                var initial = !string.IsNullOrEmpty(c.Author) ? c.Author.Substring(0, 1).ToUpper() : "U";

                return new {
                    c.Id,
                    c.Content,
                    c.Author,
                    c.AuthorRole,
                    avatarUrl,
                    initial,
                    isVerified = user != null && (user.IsVerified || string.Equals(user.VerificationStatus, "Active", StringComparison.OrdinalIgnoreCase) || string.Equals(user.VerificationStatus, "Approved", StringComparison.OrdinalIgnoreCase)),
                    createdAt = c.CreatedAt.ToString("M/d/yyyy, h:mm:ss tt")
                };
            });

            return Json(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Comment content cannot be empty." });
            }

            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null)
            {
                return Json(new { success = false, message = "Post not found." });
            }

            var userName = User.Identity?.Name ?? "Anonymous";
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            var comment = new ForumComment
            {
                PostId = postId,
                Content = content.Trim(),
                Author = userName,
                AuthorRole = userRole,
                CreatedAt = DateTime.UtcNow
            };

            _context.ForumComments.Add(comment);
            post.Replies += 1;

            if (!string.IsNullOrEmpty(post.Author) && !post.Author.Equals(userName, StringComparison.OrdinalIgnoreCase))
            {
                var notification = new Notification
                {
                    User = post.Author,
                    Sender = userName,
                    Post = post.Title,
                    Type = "Comment",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    __v = 0
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            var currentUserId = GetCurrentUserId();
            User? currentUser = null;
            if (currentUserId.HasValue)
            {
                currentUser = await _context.Users.FindAsync(currentUserId.Value);
            }
            else
            {
                currentUser = await FindUserByAuthorAsync(userName);
            }

            var avatarUrl = (currentUser != null && !string.IsNullOrWhiteSpace(currentUser.Picture))
                ? _cloudinaryService.GetAvatarUrl(currentUser.Picture, 80)
                : null;

            var initial = !string.IsNullOrEmpty(userName) ? userName.Substring(0, 1).ToUpper() : "U";

            return Json(new { 
                success = true, 
                replies = post.Replies,
                comment = new {
                    comment.Id,
                    comment.Content,
                    comment.Author,
                    comment.AuthorRole,
                    avatarUrl,
                    initial,
                    isVerified = currentUser != null && (currentUser.IsVerified || string.Equals(currentUser.VerificationStatus, "Active", StringComparison.OrdinalIgnoreCase) || string.Equals(currentUser.VerificationStatus, "Approved", StringComparison.OrdinalIgnoreCase)),
                    createdAt = comment.CreatedAt.ToString("M/d/yyyy, h:mm:ss tt")
                }
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserProfile(string author)
        {
            if (string.IsNullOrWhiteSpace(author))
            {
                return Json(new { success = false, message = "Author identifier is required." });
            }

            var cleanAuthor = author.Trim();
            var user = await FindUserByAuthorAsync(cleanAuthor);

            if (user == null)
            {
                var postCount = await _context.ForumPosts.CountAsync(p => p.Author == cleanAuthor);
                return Json(new {
                    success = true,
                    user = new {
                        fullName = cleanAuthor,
                        role = cleanAuthor.Equals("System Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Community Member",
                        picture = (string?)null,
                        avatarUrl = (string?)null,
                        initial = string.IsNullOrEmpty(cleanAuthor) ? "U" : cleanAuthor.Substring(0, 1).ToUpper(),
                        isVerified = cleanAuthor.Equals("System Admin", StringComparison.OrdinalIgnoreCase),
                        verificationStatus = "Active",
                        email = "",
                        city = "",
                        area = "",
                        location = "",
                        bio = "Active participant in the Onudhabon community discussions.",
                        educationLevel = "",
                        major = "",
                        institution = "",
                        universityName = "",
                        memberSince = "Community Member",
                        totalPosts = postCount,
                        totalLectures = 0
                    }
                });
            }

            var avatarUrl = !string.IsNullOrWhiteSpace(user.Picture)
                ? _cloudinaryService.GetAvatarUrl(user.Picture, 180)
                : null;

            var totalPosts = await _context.ForumPosts.CountAsync(p =>
                p.Author == user.FullName || p.Author == user.Email);

            var totalLectures = await _context.Lectures.CountAsync(l =>
                l.Instructor == user.FullName || l.Instructor == user.Email);

            return Json(new {
                success = true,
                user = new {
                    id = user.Id,
                    fullName = user.FullName,
                    role = user.Role,
                    picture = user.Picture,
                    avatarUrl = avatarUrl,
                    initial = string.IsNullOrEmpty(user.FullName) ? "U" : user.FullName.Substring(0, 1).ToUpper(),
                    isVerified = user.IsVerified || string.Equals(user.VerificationStatus, "Active", StringComparison.OrdinalIgnoreCase) || string.Equals(user.VerificationStatus, "Approved", StringComparison.OrdinalIgnoreCase),
                    verificationStatus = user.VerificationStatus ?? "Pending",
                    email = user.Email,
                    city = user.City,
                    area = user.Area,
                    location = user.Location,
                    bio = user.Bio,
                    educationLevel = user.EducationLevel,
                    major = user.Major,
                    institution = user.Institution,
                    universityName = user.UniversityName,
                    memberSince = user.CreatedAt.ToString("MMMM yyyy"),
                    totalPosts = totalPosts,
                    totalLectures = totalLectures
                }
            });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetNotifications()
        {
            var userName = User.Identity?.Name;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(userEmail) && string.IsNullOrEmpty(userId))
                return Json(new List<object>());

            var userPosts = await _context.ForumPosts.ToListAsync();
            var notifications = await _context.Notifications
                .Where(n => (userName != null && n.User == userName) ||
                            (userEmail != null && n.User == userEmail) ||
                            (userId != null && n.User == userId))
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .ToListAsync();

            var result = notifications.Select(n => {
                var targetPost = userPosts.FirstOrDefault(p => p.Title == n.Post);
                return new {
                    n.Id,
                    n.Sender,
                    n.Post,
                    n.Type,
                    n.IsRead,
                    postId = targetPost?.Id ?? 0,
                    createdAt = n.CreatedAt.ToString("M/d/yyyy, h:mm tt")
                };
            });

            return Json(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MarkSingleNotificationAsRead(int id)
        {
            var userName = User.Identity?.Name;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(userEmail) && string.IsNullOrEmpty(userId))
                return Json(new { success = false });

            var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && 
                ((userName != null && n.User == userName) || 
                 (userEmail != null && n.User == userEmail) || 
                 (userId != null && n.User == userId)));

            if (notif != null)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MarkNotificationsAsRead()
        {
            var userName = User.Identity?.Name;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(userEmail) && string.IsNullOrEmpty(userId))
                return Json(new { success = false });

            var unread = await _context.Notifications
                .Where(n => !n.IsRead && 
                    ((userName != null && n.User == userName) || 
                     (userEmail != null && n.User == userEmail) || 
                     (userId != null && n.User == userId)))
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
