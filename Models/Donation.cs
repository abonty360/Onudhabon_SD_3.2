using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onudhabon_ISD.Models
{
    [Table("Donations")]
    public class Donation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Donor name is required")]
        [MaxLength(150)]
        [Display(Name = "Donor Name")]
        public string DonorName { get; set; } = string.Empty;

        [MaxLength(200)]
        [EmailAddress]
        [Display(Name = "Donor Email")]
        public string? DonorEmail { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [MaxLength(50)]
        [Display(Name = "Donor Phone")]
        public string DonorPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Donation amount is required")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(10, 500000, ErrorMessage = "Donation amount must be between ৳10 and ৳500,000")]
        [Display(Name = "Amount (BDT)")]
        public decimal Amount { get; set; }

        [MaxLength(10)]
        [Display(Name = "Currency")]
        public string Currency { get; set; } = "BDT";

        [Required(ErrorMessage = "Donation purpose is required")]
        [MaxLength(150)]
        [Display(Name = "Cause / Purpose")]
        public string Purpose { get; set; } = "General Education Fund";

        [MaxLength(1000)]
        [Display(Name = "Message / Note")]
        public string? Message { get; set; }

        [MaxLength(50)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "bKash";

        [MaxLength(30)]
        [Display(Name = "bKash Wallet Number")]
        public string? BkashWalletNumber { get; set; }

        [MaxLength(100)]
        [Display(Name = "bKash Transaction ID (TRXID)")]
        public string? BkashTransactionId { get; set; }

        [MaxLength(100)]
        [Display(Name = "bKash Payment ID")]
        public string? BkashPaymentId { get; set; }

        [MaxLength(50)]
        [Display(Name = "Payment Status")]
        public string Status { get; set; } = "Completed";

        [Display(Name = "Anonymous Donation")]
        public bool IsAnonymous { get; set; } = false;

        [MaxLength(100)]
        [Display(Name = "Associated User ID")]
        public string? UserId { get; set; }

        [Display(Name = "Date of Donation")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Version")]
        public int? __v { get; set; } = 0;
    }
}
