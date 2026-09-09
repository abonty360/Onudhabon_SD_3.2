using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Onudhabon.Models
{
    public class LectureUploadViewModel
    {
        [Required(ErrorMessage = "Lecture title is required")]
        [MaxLength(255)]
        [Display(Name = "Lecture Title")]
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

        [Display(Name = "Video File (MP4, MKV, WebM)")]
        public IFormFile? VideoFile { get; set; }

        [MaxLength(500)]
        [Display(Name = "Existing Cloudinary Video URL (Optional)")]
        public string? ExistingVideoUrl { get; set; }

        [MaxLength(500)]
        [Display(Name = "Existing Thumbnail URL (Optional)")]
        public string? ExistingThumbnailUrl { get; set; }
    }
}