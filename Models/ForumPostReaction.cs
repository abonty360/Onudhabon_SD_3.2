using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onudhabon_ISD.Models
{
    [Table("ForumPostReactions")]
    public class ForumPostReaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Post ID")]
        public int PostId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "User ID")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Display(Name = "Reaction Type")]
        public string ReactionType { get; set; } = "Like"; // "Like" or "Dislike"

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Version")]
        public int? __v { get; set; } = 0;

        [ForeignKey("PostId")]
        public virtual ForumPost? Post { get; set; }
    }
}
