using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Onudhabon_ISD.Models
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [MaxLength(150, ErrorMessage = "Full name cannot exceed 150 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        [MaxLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [MaxLength(100)]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Region / Area is required")]
        [MaxLength(100)]
        [Display(Name = "Region / Area")]
        public string Area { get; set; } = string.Empty;

        [MaxLength(255)]
        [Display(Name = "Detailed Address")]
        public string? Location { get; set; }

        [Range(10, 120, ErrorMessage = "Please enter a valid age between 10 and 120")]
        [Display(Name = "Age")]
        public int? Age { get; set; }

        [MaxLength(50)]
        [Display(Name = "NID / Identification Number")]
        public string? NidNumber { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Short Bio / Summary")]
        public string? Bio { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Volunteer Motivation")]
        public string? VolunteerReason { get; set; }

        // Education details
        [MaxLength(100)]
        [Display(Name = "Highest Education Level")]
        public string? EducationLevel { get; set; }

        [MaxLength(200)]
        [Display(Name = "Institution")]
        public string? Institution { get; set; }

        [MaxLength(150)]
        [Display(Name = "Major / Field of Study")]
        public string? Major { get; set; }

        [MaxLength(100)]
        [Display(Name = "Currently Studying")]
        public string? CurrentlyStudying { get; set; }

        [MaxLength(200)]
        [Display(Name = "University / College Name")]
        public string? UniversityName { get; set; }

        [MaxLength(20)]
        [Display(Name = "University Passing / Batch Year")]
        public string? UniversityPassingYear { get; set; }

        [MaxLength(200)]
        [Display(Name = "HSC / College Institute")]
        public string? HscInstitute { get; set; }

        [MaxLength(20)]
        [Display(Name = "HSC Passing Year")]
        public string? HscPassingYear { get; set; }

        [MaxLength(200)]
        [Display(Name = "SSC / School Institute")]
        public string? SscInstitute { get; set; }

        [MaxLength(20)]
        [Display(Name = "SSC Passing Year")]
        public string? SscPassingYear { get; set; }

        [Display(Name = "Profile Photo")]
        public IFormFile? PictureFile { get; set; }

        [Display(Name = "Certificate / Education Document")]
        public IFormFile? CertificatePictureFile { get; set; }
    }
}
