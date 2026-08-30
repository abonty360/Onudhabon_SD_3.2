using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onudhabon_ISD.Models
{
    [Table("ForumPosts")]
    public class ForumPost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(255)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(4000)]
        [Display(Name = "Content")]
        public string? Content { get; set; }

        [MaxLength(150)]
        [Display(Name = "Author")]
        public string? Author { get; set; }

        [MaxLength(50)]
        [Display(Name = "Author Role")]
        public string? AuthorRole { get; set; }

        [MaxLength(255)]
        [Display(Name = "Tags")]
        public string? Tags { get; set; }

        [MaxLength(100)]
        [Display(Name = "Category")]
        public string? Category { get; set; }

        [Display(Name = "Likes")]
        public int Likes { get; set; } = 0;

        [Display(Name = "Dislikes")]
        public int Dislikes { get; set; } = 0;

        [Display(Name = "Replies")]
        public int Replies { get; set; } = 0;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Version")]
        public int? __v { get; set; } = 0;
    }
}
