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

        [Display(Name = "Age")]
        public int Age { get; set; } = 14;

        [Display(Name = "Attendance Percentage")]
        [Range(0, 100)]
        public int AttendancePercentage { get; set; } = 90;

        [Display(Name = "Progress Percentage")]
        [Range(0, 100)]
        public int ProgressPercentage { get; set; } = 75;

        [MaxLength(2000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [MaxLength(500)]
        [Display(Name = "Photo URL")]
        public string? PhotoUrl { get; set; }

        [MaxLength(4000)]
        [Display(Name = "Subject Progress (JSON)")]
        public string? SubjectProgressJson { get; set; }

        [NotMapped]
        public List<StudentSubjectProgress> SubjectProgressList
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SubjectProgressJson))
                    return new List<StudentSubjectProgress>();
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<StudentSubjectProgress>>(SubjectProgressJson)
                           ?? new List<StudentSubjectProgress>();
                }
                catch
                {
                    return new List<StudentSubjectProgress>();
                }
            }
            set
            {
                SubjectProgressJson = value != null ? System.Text.Json.JsonSerializer.Serialize(value) : null;
            }
        }

        public void SyncProgressFromSubjects()
        {
            var list = SubjectProgressList;
            if (list.Any())
            {
                int totalPlanned = list.Sum(s => Math.Max(1, s.TotalLectures));
                int totalDone = list.Sum(s => s.CompletedLectures);
                CompletedClasses = totalDone;
                if (totalPlanned > 0)
                {
                    ProgressPercentage = (int)Math.Clamp(Math.Round((double)totalDone / totalPlanned * 100), 0, 100);
                }
            }
        }

        [Display(Name = "Last Activity Date")]
        public DateTime? LastActivityDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Version")]
        public int? __v { get; set; } = 0;
    }

    public class StudentSubjectProgress
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string SubjectName { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("totalLectures")]
        public int TotalLectures { get; set; } = 12;

        [System.Text.Json.Serialization.JsonPropertyName("completedLectures")]
        public int CompletedLectures { get; set; } = 0;

        [System.Text.Json.Serialization.JsonIgnore]
        public int ProgressPercentage => TotalLectures > 0
            ? (int)Math.Clamp(Math.Round((double)CompletedLectures / TotalLectures * 100), 0, 100)
            : 0;
    }
}
