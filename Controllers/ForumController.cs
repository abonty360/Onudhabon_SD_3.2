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

        public ForumController(ApplicationDbContext context)
        {
            _context = context;
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var posts = await _context.ForumPosts
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

            var post = new ForumPost
            {
                Title = title.Trim(),
                Content = content.Trim(),
                Author = userName,
                AuthorRole = userRole,
                Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
                Tags = string.IsNullOrWhiteSpace(tags) ? "#discussion" : (tags.StartsWith("#") ? tags.Trim() : "#" + tags.Trim()),
                CreatedAt = DateTime.UtcNow,
                Likes = 0,
                Dislikes = 0,
                Replies = 0
            };

            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Forum post created successfully!";
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
                .Select(c => new {
                    c.Id,
                    c.Content,
                    c.Author,
                    c.AuthorRole,
                    createdAt = c.CreatedAt.ToString("M/d/yyyy, h:mm:ss tt")
                })
                .ToListAsync();
            return Json(comments);
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
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                replies = post.Replies,
                comment = new {
                    comment.Id,
                    comment.Content,
                    comment.Author,
                    comment.AuthorRole,
                    createdAt = comment.CreatedAt.ToString("M/d/yyyy, h:mm:ss tt")
                }
            });
        }
    }
}
