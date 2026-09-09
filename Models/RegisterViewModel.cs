using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Onudhabon.Models
{
    public class RegisterViewModel
    {
        // ----------------- Core Credentials -----------------
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(150, ErrorMessage = "Full name cannot exceed 150 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [MaxLength(256, ErrorMessage = "Email address cannot exceed 256 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [MaxLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a role")]
        [MaxLength(50)]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a city")]
        [MaxLength(100)]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a region/area")]
        [MaxLength(100)]
        [Display(Name = "Region / Area")]
        public string Area { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Specific location cannot exceed 255 characters")]
        [Display(Name = "Street / House / Detailed Address")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // ----------------- Personal Details -----------------
        [Range(10, 120, ErrorMessage = "Please enter a valid age between 10 and 120")]
        [Display(Name = "Age")]
        public int? Age { get; set; }

        [MaxLength(50, ErrorMessage = "NID Number cannot exceed 50 characters")]
        [Display(Name = "NID / Identification Number")]
        public string? NidNumber { get; set; }

        [MaxLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
        [Display(Name = "Short Bio / Summary")]
        public string? Bio { get; set; }

        [Display(Name = "Profile Photo")]
        public IFormFile? PictureFile { get; set; }

        [Display(Name = "Why do you want to be a volunteer / mentor?")]
        [MaxLength(1000, ErrorMessage = "Volunteer reason cannot exceed 1000 characters")]
        public string? VolunteerReason { get; set; }

        // ----------------- Educational Background -----------------
        [MaxLength(100, ErrorMessage = "Education level cannot exceed 100 characters")]
        [Display(Name = "Highest Education Level")]
        public string? EducationLevel { get; set; }

        [MaxLength(150, ErrorMessage = "Major / Field of study cannot exceed 150 characters")]
        [Display(Name = "Major / Field of Study")]
        public string? Major { get; set; }

        [MaxLength(100)]
        [Display(Name = "Currently Studying")]
        public string? CurrentlyStudying { get; set; }

        [MaxLength(20)]
        [Display(Name = "SSC Passing Year")]
        public string? SscPassingYear { get; set; }

        [MaxLength(200)]
        [Display(Name = "SSC Institute")]
        public string? SscInstitute { get; set; }

        [MaxLength(20)]
        [Display(Name = "HSC Passing Year")]
        public string? HscPassingYear { get; set; }

        [MaxLength(200)]
        [Display(Name = "HSC Institute")]
        public string? HscInstitute { get; set; }

        [MaxLength(200)]
        [Display(Name = "University / College Name")]
        public string? UniversityName { get; set; }

        [MaxLength(20)]
        [Display(Name = "University Passing / Expected Year")]
        public string? UniversityPassingYear { get; set; }

        [Display(Name = "Certificate / Degree Document")]
        public IFormFile? CertificatePictureFile { get; set; }

        // ----------------- Terms -----------------
        [Display(Name = "Terms & Conditions")]
        public bool AgreeToTerms { get; set; }
    }
}