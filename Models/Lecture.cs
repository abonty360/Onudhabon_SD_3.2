using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onudhabon.Models
{
    [Table("Lectures")]
    public class Lecture
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(255)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [MaxLength(150)]
        [Display(Name = "Instructor")]
        public string? Instructor { get; set; }

        [MaxLength(50)]
        [Display(Name = "Version")]
        public string? Version { get; set; }

        [MaxLength(50)]
        [Display(Name = "Class Level")]
        public string? ClassLevel { get; set; }

        [MaxLength(100)]
        [Display(Name = "Subject")]
        public string? Subject { get; set; }

        [MaxLength(150)]
        [Display(Name = "Topic")]
        public string? Topic { get; set; }

        [MaxLength(500)]
        [Display(Name = "Thumbnail")]
        public string? Thumbnail { get; set; }

        [MaxLength(500)]
        [Display(Name = "Video URL")]
        public string? VideoUrl { get; set; }

        [MaxLength(50)]
        [Display(Name = "Status")]
        public string? Status { get; set; } = "Active";

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Version")]
        public int? __v { get; set; } = 0;
    }
}
