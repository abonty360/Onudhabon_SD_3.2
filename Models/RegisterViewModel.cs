using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Onudhabon.Models
{
    public class RegisterViewModel
    {
        // ================= Section 1: Account & Credentials =================
        [Required(ErrorMessage = "Please enter your full name")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose your role")]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Profile Photo")]
        public IFormFile? ProfilePhoto { get; set; }

        [Required(ErrorMessage = "Please enter a password")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Backward compatibility
        public string Username { get; set; } = string.Empty;

        // ================= Section 2: Personal & Location Information =================
        [Display(Name = "Age")]
        public string? Age { get; set; }

        [Display(Name = "NID / Birth Certificate No")]
        public string? NidOrBirthCert { get; set; }

        [Required(ErrorMessage = "Please select your city")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select your region/area")]
        [Display(Name = "Region / Area")]
        public string? RegionOrArea { get; set; }

        [Display(Name = "Detailed Address / Location")]
        public string? DetailedAddress { get; set; }

        [Display(Name = "Short Bio")]
        [StringLength(500, ErrorMessage = "Short bio cannot exceed 500 characters")]
        public string? ShortBio { get; set; }

        // ================= Section 3: Academic & Educational Background =================
        [Display(Name = "Highest Education Level")]
        public string? HighestEducationLevel { get; set; }

        [Display(Name = "Currently Enrolled / Studying?")]
        public string? CurrentlyEnrolled { get; set; }

        [Display(Name = "Major / Department / Field of Study")]
        public string? MajorOrFieldOfStudy { get; set; }

        [Display(Name = "University / College")]
        public string? UniversityOrCollege { get; set; }

        [Display(Name = "University Passing / Expected Year")]
        public string? UniversityPassingYear { get; set; }

        [Display(Name = "HSC / College Institute")]
        public string? HscInstitute { get; set; }

        [Display(Name = "HSC Passing Year")]
        public string? HscPassingYear { get; set; }

        [Display(Name = "SSC / School Institute")]
        public string? SscInstitute { get; set; }

        [Display(Name = "SSC Passing Year")]
        public string? SscPassingYear { get; set; }

        [Display(Name = "Certificate / Student ID / Document")]
        public IFormFile? EducationDocument { get; set; }

        // ================= Section 4: Community & Agreement =================
        [Display(Name = "Why do you want to join / volunteer?")]
        [StringLength(1000, ErrorMessage = "Motivation cannot exceed 1000 characters")]
        public string? VolunteerMotivation { get; set; }

        // ================= Terms =================
        [Display(Name = "I agree to the Terms and Conditions and acknowledge the Privacy Policy")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the Terms and Conditions and acknowledge the Privacy Policy to continue")]
        public bool AgreeToTerms { get; set; } = false;
    }
}
