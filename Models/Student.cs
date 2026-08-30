using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onudhabon_ISD.Models
{
    [Table("Students")]
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(100)]
        [Display(Name = "Birth Certificate ID")]
        public string? BirthCertificateId { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [MaxLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(300)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [MaxLength(150)]
        [Display(Name = "Father's Name")]
        public string? FatherName { get; set; }

        [MaxLength(150)]
        [Display(Name = "Mother's Name")]
        public string? MotherName { get; set; }

        [MaxLength(50)]
        [Display(Name = "Class Level")]
        public string? ClassLevel { get; set; }

        [MaxLength(500)]
        [Display(Name = "Subjects")]
        public string? Subjects { get; set; }

        [MaxLength(20)]
        [Display(Name = "Enrollment Year")]
        public string? EnrollmentYear { get; set; }

        [MaxLength(500)]
        [Display(Name = "Consent Letter URL")]
        public string? ConsentLetterUrl { get; set; }

        [MaxLength(50)]
        [Display(Name = "Guardian ID")]
        public string? GuardianId { get; set; }

        [MaxLength(150)]
        [Display(Name = "Guardian Name")]
        public string? GuardianName { get; set; }

        [MaxLength(50)]
        [Display(Name = "Status")]
        public string? Status { get; set; } = "Pending";

        [Display(Name = "Completed Classes")]
        public int CompletedClasses { get; set; } = 0;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Version")]
        public int? __v { get; set; } = 0;
    }
}
