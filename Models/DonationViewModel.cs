using System.ComponentModel.DataAnnotations;

namespace Onudhabon.Models
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

        public string PaymentMethod { get; set; } = "bKash";

        public string? BkashWalletNumber { get; set; }

        public string? CardNumber { get; set; }

        public string? CardExpiry { get; set; }

        public string? CardCvv { get; set; }

        public string? CardHolderName { get; set; }

        public string? OtpCode { get; set; }

        public string? Pin { get; set; }
    }

    public class DonationReceiptViewModel
    {
        public Donation Donation { get; set; } = new();
        public bool IsRecent { get; set; } = true;
    }
}
