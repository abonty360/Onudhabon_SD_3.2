using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onudhabon_ISD.Models
{
    [Table("ForumComments")]
    public class ForumComment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Post ID")]
        public int PostId { get; set; }

        [MaxLength(150)]
        [Display(Name = "Author")]
        public string? Author { get; set; }

        [Required(ErrorMessage = "Comment content cannot be empty")]
        [MaxLength(4000)]
        [Display(Name = "Content")]
        public string Content { get; set; } = string.Empty;

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
