using System.ComponentModel.DataAnnotations;

namespace Onudhabon.Models
{
    public class VerifyOtpViewModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the 6-digit OTP.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be 6 numeric digits.")]
        [Display(Name = "6-Digit Verification Code")]
        public string Otp { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
