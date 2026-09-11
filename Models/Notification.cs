using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onudhabon.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(100)]
        [Display(Name = "User")]
        public string? User { get; set; }

        [MaxLength(100)]
        [Display(Name = "Sender")]
        public string? Sender { get; set; }

        [MaxLength(100)]
        [Display(Name = "Post")]
        public string? Post { get; set; }

        [MaxLength(50)]
        [Display(Name = "Type")]
        public string? Type { get; set; }

        [Display(Name = "Is Read")]
        public bool IsRead { get; set; } = false;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Version")]
        public int? __v { get; set; } = 0;
    }
}
