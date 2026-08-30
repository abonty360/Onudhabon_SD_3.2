using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Onudhabon_ISD.Models
{
    public class MaterialUploadViewModel
    {
        [Required(ErrorMessage = "Material title is required")]
        [MaxLength(255)]
        [Display(Name = "Material Title")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [MaxLength(150)]
        [Display(Name = "Instructor / Educator")]
        public string? Instructor { get; set; }

        [MaxLength(50)]
        [Display(Name = "Medium / Version")]
        public string? Version { get; set; } = "Bangla";

        [Required(ErrorMessage = "Class level is required")]
        [MaxLength(50)]
        [Display(Name = "Class Level")]
        public string ClassLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required")]
        [MaxLength(100)]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Topic is required")]
        [MaxLength(150)]
        [Display(Name = "Topic")]
        public string Topic { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a material document file (PDF/DOCX) to upload.")]
        [Display(Name = "Material Document File (PDF, DOCX, etc.)")]
        public IFormFile? MaterialFile { get; set; }
    }
}
