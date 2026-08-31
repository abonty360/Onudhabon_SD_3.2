using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Onudhabon_ISD.Models
{
    public class StudentEnrollmentViewModel
    {
        [Required(ErrorMessage = "Student's Full Name is required.")]
        [MaxLength(150, ErrorMessage = "Full Name cannot exceed 150 characters.")]
        [Display(Name = "Student Full Name *")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Birth Certificate ID is required.")]
        [MaxLength(100, ErrorMessage = "Birth Certificate ID cannot exceed 100 characters.")]
        [Display(Name = "Birth Certificate ID *")]
        public string BirthCertificateId { get; set; } = string.Empty;

        [MaxLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        [Display(Name = "Residential Address / Area")]
        public string? Address { get; set; }

        [MaxLength(150, ErrorMessage = "Father's Name cannot exceed 150 characters.")]
        [Display(Name = "Father's Name")]
        public string? FatherName { get; set; }

        [MaxLength(150, ErrorMessage = "Mother's Name cannot exceed 150 characters.")]
        [Display(Name = "Mother's Name")]
        public string? MotherName { get; set; }

        [Required(ErrorMessage = "Class Level is required.")]
        [MaxLength(50)]
        [Display(Name = "Class Level *")]
        public string ClassLevel { get; set; } = string.Empty;

        [Range(4, 25, ErrorMessage = "Age must be between 4 and 25.")]
        [Display(Name = "Student Age (Years)")]
        public int? Age { get; set; } = 14;

        [MaxLength(500)]
        [Display(Name = "Enrolled Subjects")]
        public string? Subjects { get; set; }

        [MaxLength(20)]
        [Display(Name = "Enrollment Year")]
        public string? EnrollmentYear { get; set; } = DateTime.UtcNow.Year.ToString();

        [Display(Name = "Legal Guardian Consent Letter (PDF/Image)")]
        public IFormFile? ConsentLetterFile { get; set; }

        [Display(Name = "Local Guardian Name")]
        public string? GuardianName { get; set; }

        [Display(Name = "Local Guardian ID")]
        public string? GuardianId { get; set; }

        [Display(Name = "Certification & Declaration")]
        public bool DeclarationCertified { get; set; } = true;
    }
}
