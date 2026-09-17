using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onudhabon.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [MaxLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [MaxLength(256)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        [MaxLength(50)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [MaxLength(50)]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [MaxLength(100)]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Region/Area is required")]
        [MaxLength(100)]
        [Display(Name = "Region / Area")]
        public string Area { get; set; } = string.Empty;

        [MaxLength(255)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Why do you want to be a volunteer?")]
        public string? VolunteerReason { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Display(Name = "Terms & Conditions")]
        public bool AgreeToTerms { get; set; } = true;

        [MaxLength(1000)]
        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        [MaxLength(500)]
        [Display(Name = "Profile Picture")]
        public string? Picture { get; set; }

        [Display(Name = "Is Restricted")]
        public bool IsRestricted { get; set; } = false;

        [Display(Name = "Is Verified")]
        public bool IsVerified { get; set; } = false;

        [MaxLength(50)]
        [Display(Name = "Verification Status")]
        public string? VerificationStatus { get; set; } = "Pending";

        [Display(Name = "Is Email Verified")]
        public bool IsEmailVerified { get; set; } = false;

        [MaxLength(100)]
        [Display(Name = "Email Verification Token")]
        public string? EmailVerificationToken { get; set; }

        [Display(Name = "Email Verification Token Expiry")]
        public DateTime? EmailVerificationTokenExpiry { get; set; }

        [MaxLength(10)]
        [Display(Name = "Email OTP")]
        public string? EmailOtp { get; set; }

        [Display(Name = "Email OTP Expiry")]
        public DateTime? EmailOtpExpiry { get; set; }

        [MaxLength(50)]
        [Display(Name = "NID Number")]
        public string? NidNumber { get; set; }

        [MaxLength(500)]
        [Display(Name = "Certificate Picture")]
        public string? CertificatePicture { get; set; }

        [MaxLength(100)]
        [Display(Name = "Education Level")]
        public string? EducationLevel { get; set; }

        [MaxLength(200)]
        [Display(Name = "Institution")]
        public string? Institution { get; set; }

        [MaxLength(150)]
        [Display(Name = "Major")]
        public string? Major { get; set; }

        [Display(Name = "Age")]
        public int? Age { get; set; }

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
        [Display(Name = "University Name")]
        public string? UniversityName { get; set; }

        [MaxLength(20)]
        [Display(Name = "University Passing Year")]
        public string? UniversityPassingYear { get; set; }

        [MaxLength(100)]
        [Display(Name = "Currently Studying")]
        public string? CurrentlyStudying { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Version")]
        public int? __v { get; set; } = 0;
    }
}
