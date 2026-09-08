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

        [Required]
        [MaxLength(150)]
        [Display(Name = "Donor Name")]
        public string DonorName { get; set; } = string.Empty;

        [MaxLength(256)]
        [EmailAddress]
        [Display(Name = "Donor Email")]
        public string? DonorEmail { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Mobile Phone Number")]
        public string DonorPhone { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Donation Amount")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(255)]
        [Display(Name = "Program Purpose")]
        public string Purpose { get; set; } = "General Education & Child Support Fund";

        [MaxLength(1000)]
        [Display(Name = "Dedication / Message")]
        public string? Message { get; set; }

        [Display(Name = "Anonymous Donation")]
        public bool IsAnonymous { get; set; } = false;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Transaction ID")]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "bKash";

        [MaxLength(50)]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Completed";

        [MaxLength(50)]
        [Display(Name = "bKash Wallet")]
        public string? BkashWalletNumber { get; set; }

        private string? _bkashTransactionId;
        [MaxLength(100)]
        [Display(Name = "bKash Transaction ID")]
        public string? BkashTransactionId
        {
            get => !string.IsNullOrWhiteSpace(_bkashTransactionId) ? _bkashTransactionId : TransactionId;
            set => _bkashTransactionId = value;
        }

        private string? _bkashPaymentId;
        [MaxLength(100)]
        [Display(Name = "bKash Payment Reference ID")]
        public string? BkashPaymentId
        {
            get => !string.IsNullOrWhiteSpace(_bkashPaymentId) ? _bkashPaymentId : InvoiceNumber;
            set => _bkashPaymentId = value;
        }

        [Display(Name = "Date of Donation")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? __v { get; set; } = 0;
    }
}
