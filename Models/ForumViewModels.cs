using System.ComponentModel.DataAnnotations;

namespace Onudhabon.Models
{
    public class ForumIndexViewModel
    {
        public List<ForumPostItemViewModel> Posts { get; set; } = new();
        public bool IsAdmin { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? CurrentUserId { get; set; }
        public string? CurrentUserName { get; set; }
        public string? SearchQuery { get; set; }
        public string? ActiveTab { get; set; } = "all";
    }

    public class ForumPostItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? Author { get; set; }
        public string? Tags { get; set; }
        public string Status { get; set; } = "Pending";
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UserReaction { get; set; } // "Like", "Dislike", or null
        public bool IsAuthor { get; set; }
        public List<ForumCommentItemViewModel> Comments { get; set; } = new();
    }

    public class ForumCommentItemViewModel
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string? Author { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateForumPostViewModel
    {
        [Required(ErrorMessage = "Post title is required")]
        [MaxLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        [Display(Name = "Post Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Post content is required")]
        [MaxLength(4000, ErrorMessage = "Content cannot exceed 4000 characters")]
        [Display(Name = "Post Content")]
        public string Content { get; set; } = string.Empty;

        [MaxLength(255)]
        [Display(Name = "Tags (comma-separated, optional)")]
        public string? Tags { get; set; }
    }

    public class AddForumCommentViewModel
    {
        [Required]
        public int PostId { get; set; }

        [Required(ErrorMessage = "Comment cannot be empty")]
        [MaxLength(4000, ErrorMessage = "Comment cannot exceed 4000 characters")]
        public string Content { get; set; } = string.Empty;
    }
}
