using System.ComponentModel.DataAnnotations;

namespace Onudhabon_ISD.Models
{
    public class DonationInitiateViewModel
    {
        [Required(ErrorMessage = "Please specify a contribution amount")]
        [Range(10, 500000, ErrorMessage = "Donation amount must be between ৳10 and ৳500,000")]
        [Display(Name = "Contribution Amount (BDT)")]
        public decimal Amount { get; set; } = 500;

        [Required(ErrorMessage = "Please select a program allocation purpose")]
        [MaxLength(255)]
        [Display(Name = "Program Allocation")]
        public string Purpose { get; set; } = "General Education & Child Support Fund";

        [Required(ErrorMessage = "Donor full name is required")]
        [MaxLength(150, ErrorMessage = "Full name cannot exceed 150 characters")]
        [Display(Name = "Full Name")]
        public string DonorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile phone number is required")]
        [MaxLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
        [Display(Name = "Mobile Number")]
        public string DonorPhone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Please provide a valid email address")]
        [MaxLength(256)]
        [Display(Name = "Email Address")]
        public string? DonorEmail { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Dedication / Program Note")]
        public string? Message { get; set; }

        [Display(Name = "Donate Anonymously")]
        public bool IsAnonymous { get; set; } = false;
    }

    public class BkashPaymentRequest
    {
        public string DonorName { get; set; } = string.Empty;
        public string? DonorEmail { get; set; }
        public string DonorPhone { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string? Message { get; set; }
        public bool IsAnonymous { get; set; }
        public string BkashWalletNumber { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
    }
}
