using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon_ISD.Data;
using Onudhabon_ISD.Models;
using System.Security.Claims;

namespace Onudhabon_ISD.Controllers
{
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ForumController> _logger;

        public ForumController(ApplicationDbContext context, ILogger<ForumController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Forum
        [HttpGet]
        public async Task<IActionResult> Index(string? search = null, string? filter = null)
        {
            var isAuthenticated = User.Identity != null && User.Identity.IsAuthenticated;
            var isAdmin = User.IsInRole("Admin") || string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "Admin", StringComparison.OrdinalIgnoreCase);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserName = User.Identity?.Name;
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            IQueryable<ForumPost> query = _context.ForumPosts
                .Include(p => p.Comments)
                .Include(p => p.Reactions)
                .AsQueryable();

            // Visibility filtering:
            // - Admins see all posts (Active + Pending)
            // - Authenticated regular users see all Active posts + their own Pending posts
            // - Unauthenticated users see only Active posts
            if (isAdmin)
            {
                // Admin can see everything
                if (filter == "pending")
                {
                    query = query.Where(p => p.Status == "Pending");
                }
                else if (filter == "active")
                {
                    query = query.Where(p => p.Status == "Active");
                }
            }
            else if (isAuthenticated)
            {
                if (filter == "myposts")
                {
                    query = query.Where(p => p.Author == currentUserName || (currentUserEmail != null && p.Author == currentUserEmail));
                }
                else
                {
                    query = query.Where(p => p.Status == "Active" ||
                        (p.Status == "Pending" && (p.Author == currentUserName || (currentUserEmail != null && p.Author == currentUserEmail))));
                }
            }
            else
            {
                // Unauthenticated visitors only see approved active posts
                query = query.Where(p => p.Status == "Active");
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(term) ||
                    (p.Content != null && p.Content.ToLower().Contains(term)) ||
                    (p.Author != null && p.Author.ToLower().Contains(term)) ||
                    (p.Tags != null && p.Tags.ToLower().Contains(term)));
            }

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var postViewModels = posts.Select(p =>
            {
                string? userReaction = null;
                if (isAuthenticated)
                {
                    var reaction = p.Reactions.FirstOrDefault(r =>
                        r.UserId == currentUserId ||
                        (currentUserName != null && r.UserId == currentUserName) ||
                        (currentUserEmail != null && r.UserId == currentUserEmail));
                    userReaction = reaction?.ReactionType;
                }

                var isAuthor = isAuthenticated && (
                    string.Equals(p.Author, currentUserName, StringComparison.OrdinalIgnoreCase) ||
                    (currentUserEmail != null && string.Equals(p.Author, currentUserEmail, StringComparison.OrdinalIgnoreCase)));

                return new ForumPostItemViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    Author = p.Author,
                    Tags = p.Tags,
                    Status = p.Status,
                    LikeCount = p.LikeCount,
                    DislikeCount = p.DislikeCount,
                    CommentCount = p.Comments.Count,
                    CreatedAt = p.CreatedAt,
                    UserReaction = userReaction,
                    IsAuthor = isAuthor,
                    Comments = p.Comments
                        .OrderBy(c => c.CreatedAt)
                        .Select(c => new ForumCommentItemViewModel
                        {
                            Id = c.Id,
                            PostId = c.PostId,
                            Author = c.Author,
                            Content = c.Content,
                            CreatedAt = c.CreatedAt
                        }).ToList()
                };
            }).ToList();

            var viewModel = new ForumIndexViewModel
            {
                Posts = postViewModels,
                IsAdmin = isAdmin,
                IsAuthenticated = isAuthenticated,
                CurrentUserId = currentUserId,
                CurrentUserName = currentUserName,
                SearchQuery = search,
                ActiveTab = filter ?? "all"
            };

            return View(viewModel);
        }

        // GET: /Forum/Create
        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View(new CreateForumPostViewModel());
        }

        // POST: /Forum/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateForumPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUserName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "User";

            var post = new ForumPost
            {
                Title = model.Title.Trim(),
                Content = model.Content.Trim(),
                Tags = string.IsNullOrWhiteSpace(model.Tags) ? null : model.Tags.Trim(),
                Author = currentUserName,
                Status = "Pending", // Always starts as Pending
                LikeCount = 0,
                DislikeCount = 0,
                Replies = 0,
                CreatedAt = DateTime.UtcNow,
                __v = 0
            };

            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your forum post has been submitted and is currently pending review by an administrator.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Forum/ApprovePost/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null)
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Post not found." });
                }
                return NotFound();
            }

            post.Status = "Active";
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            if (IsAjaxRequest())
            {
                return Json(new { success = true, status = "Active", message = $"Post '{post.Title}' approved successfully." });
            }

            TempData["SuccessMessage"] = $"Post '{post.Title}' has been approved and is now active.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Forum/DisapprovePost/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisapprovePost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null)
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Post not found." });
                }
                return NotFound();
            }

            post.Status = "Declined";
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            if (IsAjaxRequest())
            {
                return Json(new { success = true, status = "Declined", message = $"Post '{post.Title}' has been declined." });
            }

            TempData["SuccessMessage"] = $"Post '{post.Title}' has been declined.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Forum/DeletePost/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null)
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Post not found." });
                }
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin") || string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "Admin", StringComparison.OrdinalIgnoreCase);
            var currentUserName = User.Identity?.Name;
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            var isAuthor = string.Equals(post.Author, currentUserName, StringComparison.OrdinalIgnoreCase) ||
                           (currentUserEmail != null && string.Equals(post.Author, currentUserEmail, StringComparison.OrdinalIgnoreCase));

            if (!isAdmin && !isAuthor)
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, message = "You are not authorized to delete this post." });
                }
                return Forbid();
            }

            _context.ForumPosts.Remove(post);
            await _context.SaveChangesAsync();

            if (IsAjaxRequest())
            {
                return Json(new { success = true, message = "Post deleted successfully." });
            }

            TempData["SuccessMessage"] = "Post deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Forum/ToggleLike/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int id)
        {
            var post = await _context.ForumPosts
                .Include(p => p.Reactions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return Json(new { success = false, message = "Post not found." });
            }

            if (post.Status != "Active")
            {
                return Json(new { success = false, message = "Liking is disabled until the post is approved by an administrator." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "user";
            var userName = User.Identity?.Name ?? "Someone";

            var existingReaction = await _context.ForumPostReactions
                .FirstOrDefaultAsync(r => r.PostId == id && r.UserId == userId);

            string? newReactionType = null;

            if (existingReaction == null)
            {
                // Add new Like reaction
                var reaction = new ForumPostReaction
                {
                    PostId = id,
                    UserId = userId,
                    ReactionType = "Like",
                    CreatedAt = DateTime.UtcNow
                };
                _context.ForumPostReactions.Add(reaction);
                newReactionType = "Like";

                // Notify post author if not self
                await SendReactionNotificationAsync(post, userName, "Like");
            }
            else if (existingReaction.ReactionType == "Like")
            {
                // Remove existing Like
                _context.ForumPostReactions.Remove(existingReaction);
                newReactionType = null;
            }
            else
            {
                // Switch from Dislike to Like
                existingReaction.ReactionType = "Like";
                existingReaction.UpdatedAt = DateTime.UtcNow;
                newReactionType = "Like";

                // Notify post author if not self
                await SendReactionNotificationAsync(post, userName, "Like");
            }

            await _context.SaveChangesAsync();

            // Recalculate actual reaction counts
            post.LikeCount = await _context.ForumPostReactions.CountAsync(r => r.PostId == id && r.ReactionType == "Like");
            post.DislikeCount = await _context.ForumPostReactions.CountAsync(r => r.PostId == id && r.ReactionType == "Dislike");
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                likeCount = post.LikeCount,
                dislikeCount = post.DislikeCount,
                userReaction = newReactionType
            });
        }

        // POST: /Forum/ToggleDislike/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDislike(int id)
        {
            var post = await _context.ForumPosts
                .Include(p => p.Reactions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return Json(new { success = false, message = "Post not found." });
            }

            if (post.Status != "Active")
            {
                return Json(new { success = false, message = "Disliking is disabled until the post is approved by an administrator." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "user";
            var userName = User.Identity?.Name ?? "Someone";

            var existingReaction = await _context.ForumPostReactions
                .FirstOrDefaultAsync(r => r.PostId == id && r.UserId == userId);

            string? newReactionType = null;

            if (existingReaction == null)
            {
                // Add new Dislike reaction
                var reaction = new ForumPostReaction
                {
                    PostId = id,
                    UserId = userId,
                    ReactionType = "Dislike",
                    CreatedAt = DateTime.UtcNow
                };
                _context.ForumPostReactions.Add(reaction);
                newReactionType = "Dislike";

                // Notify post author if not self
                await SendReactionNotificationAsync(post, userName, "Dislike");
            }
            else if (existingReaction.ReactionType == "Dislike")
            {
                // Remove existing Dislike
                _context.ForumPostReactions.Remove(existingReaction);
                newReactionType = null;
            }
            else
            {
                // Switch from Like to Dislike
                existingReaction.ReactionType = "Dislike";
                existingReaction.UpdatedAt = DateTime.UtcNow;
                newReactionType = "Dislike";

                // Notify post author if not self
                await SendReactionNotificationAsync(post, userName, "Dislike");
            }

            await _context.SaveChangesAsync();

            // Recalculate actual reaction counts
            post.LikeCount = await _context.ForumPostReactions.CountAsync(r => r.PostId == id && r.ReactionType == "Like");
            post.DislikeCount = await _context.ForumPostReactions.CountAsync(r => r.PostId == id && r.ReactionType == "Dislike");
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                likeCount = post.LikeCount,
                dislikeCount = post.DislikeCount,
                userReaction = newReactionType
            });
        }

        // POST: /Forum/AddComment
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(AddForumCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Comment cannot be empty." });
                }
                TempData["ErrorMessage"] = "Comment cannot be empty.";
                return RedirectToAction(nameof(Index));
            }

            var post = await _context.ForumPosts
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == model.PostId);

            if (post == null)
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Post not found." });
                }
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin") || string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "Admin", StringComparison.OrdinalIgnoreCase);
            var currentUserName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "User";
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            var isAuthor = string.Equals(post.Author, currentUserName, StringComparison.OrdinalIgnoreCase) ||
                (currentUserEmail != null && string.Equals(post.Author, currentUserEmail, StringComparison.OrdinalIgnoreCase));

            // Only allow commenting on Active posts
            if (post.Status != "Active")
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Comments are disabled until the post is approved by an administrator." });
                }
                TempData["ErrorMessage"] = "Comments are disabled until the post is approved by an administrator.";
                return RedirectToAction(nameof(Index));
            }

            var comment = new ForumComment
            {
                PostId = model.PostId,
                Author = currentUserName,
                Content = model.Content.Trim(),
                CreatedAt = DateTime.UtcNow,
                __v = 0
            };

            _context.ForumComments.Add(comment);
            await _context.SaveChangesAsync();

            // Update reply count on post
            post.Replies = await _context.ForumComments.CountAsync(c => c.PostId == model.PostId);
            await _context.SaveChangesAsync();

            // Send notification to post author if not self
            if (!isAuthor && !string.IsNullOrWhiteSpace(post.Author))
            {
                var notification = new Notification
                {
                    User = post.Author,
                    Sender = currentUserName,
                    Post = post.Title,
                    Type = "Comment",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    __v = 0
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    comment = new
                    {
                        id = comment.Id,
                        author = comment.Author,
                        content = comment.Content,
                        createdAt = comment.CreatedAt.ToString("MMM dd, yyyy h:mm tt")
                    },
                    commentCount = post.Replies
                });
            }

            TempData["SuccessMessage"] = "Comment added successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task SendReactionNotificationAsync(ForumPost post, string senderName, string reactionType)
        {
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            var isSelf = string.Equals(post.Author, senderName, StringComparison.OrdinalIgnoreCase) ||
                         (currentUserEmail != null && string.Equals(post.Author, currentUserEmail, StringComparison.OrdinalIgnoreCase));

            if (!isSelf && !string.IsNullOrWhiteSpace(post.Author))
            {
                var notification = new Notification
                {
                    User = post.Author,
                    Sender = senderName,
                    Post = post.Title,
                    Type = reactionType,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    __v = 0
                };
                _context.Notifications.Add(notification);
            }
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   Request.Headers["Accept"].ToString().Contains("application/json");
        }
    }
}
