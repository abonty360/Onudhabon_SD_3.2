using System.ComponentModel.DataAnnotations;

namespace Onudhabon_ISD.Models
{
    public class DonationInitiateViewModel
    {
        [Required(ErrorMessage = "Please enter your name.")]
        [MaxLength(150)]
        [Display(Name = "Your Full Name *")]
        public string DonorName { get; set; } = string.Empty;

        [MaxLength(200)]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address (Optional)")]
        public string? DonorEmail { get; set; }

        [Required(ErrorMessage = "Please enter your mobile phone number.")]
        [MaxLength(50)]
        [RegularExpression(@"^(?:\+?880|0)1[3-9]\d{8}$", ErrorMessage = "Please enter a valid Bangladeshi mobile number (e.g. 017XXXXXXXX).")]
        [Display(Name = "Phone / Mobile Number *")]
        public string DonorPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please specify a donation amount.")]
        [Range(10, 500000, ErrorMessage = "Donation amount must be between ৳10 and ৳500,000.")]
        [Display(Name = "Donation Amount (BDT) *")]
        public decimal Amount { get; set; } = 500;

        [Required(ErrorMessage = "Please select a cause or fund.")]
        [MaxLength(150)]
        [Display(Name = "Select Cause / Purpose *")]
        public string Purpose { get; set; } = "General Education & Child Support Fund";

        [MaxLength(1000)]
        [Display(Name = "Leave a Message or Dedication (Optional)")]
        public string? Message { get; set; }

        [Display(Name = "Make this donation anonymous")]
        public bool IsAnonymous { get; set; } = false;
    }

    public class BkashPaymentRequestModel
    {
        [Required]
        public string DonorName { get; set; } = string.Empty;

        public string? DonorEmail { get; set; }

        [Required]
        public string DonorPhone { get; set; } = string.Empty;

        [Required]
        [Range(10, 500000)]
        public decimal Amount { get; set; }

        [Required]
        public string Purpose { get; set; } = string.Empty;

        public string? Message { get; set; }

        public bool IsAnonymous { get; set; }

        [Required(ErrorMessage = "bKash Account Number is required")]
        [RegularExpression(@"^(?:\+?880|0)1[3-9]\d{8}$", ErrorMessage = "Please enter a valid 11-digit bKash wallet number (01XXXXXXXXX)")]
        public string BkashWalletNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP / Verification Code is required")]
        public string OtpCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "bKash PIN is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "bKash PIN must be 5 digits")]
        public string Pin { get; set; } = string.Empty;
    }

    public class DonationReceiptViewModel
    {
        public Donation Donation { get; set; } = new();
        public bool IsRecent { get; set; } = true;
    }
}
