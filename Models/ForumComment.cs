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

        public int PostId { get; set; }

        [Required(ErrorMessage = "Comment text is required")]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Author { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? AuthorRole { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
