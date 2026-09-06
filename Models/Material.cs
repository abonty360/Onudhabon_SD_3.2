using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onudhabon_ISD.Models
{
    [Table("Materials")]
    public class Material
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
        [Display(Name = "File URL")]
        public string? FileUrl { get; set; }

        [MaxLength(50)]
        [Display(Name = "Size")]
        public string? Size { get; set; }

        [Display(Name = "Downloads")]
        public int Downloads { get; set; } = 0;

        [MaxLength(50)]
        [Display(Name = "Status")]
        public string? Status { get; set; } = "Active";

        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Display(Name = "Version")]
        public int? __v { get; set; } = 0;
    }
}