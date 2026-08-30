using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Onudhabon_ISD.Models
{
    public class SubjectDetail
    {
        [JsonPropertyName("name")]
        [Display(Name = "Subject Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("totalLectures")]
        [Display(Name = "Total Lectures")]
        public int TotalLectures { get; set; }
    }

    [Table("ClassPlans")]
    public class ClassPlan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Class Level is required")]
        [MaxLength(100)]
        [Display(Name = "Class Level")]
        public string ClassLevel { get; set; } = string.Empty;

        [Display(Name = "Subjects")]
        public List<SubjectDetail> Subjects { get; set; } = new();

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Version")]
        public int? __v { get; set; } = 0;
    }
}
