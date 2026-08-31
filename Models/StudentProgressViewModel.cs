using System.Text.Json.Serialization;

namespace Onudhabon_ISD.Models
{
    public class SubjectProgressItem
    {
        [JsonPropertyName("subjectName")]
        public string SubjectName { get; set; } = string.Empty;

        [JsonPropertyName("totalLectures")]
        public int TotalLectures { get; set; } = 12;

        [JsonPropertyName("completedLectures")]
        public int CompletedLectures { get; set; } = 0;

        [JsonPropertyName("progressPercentage")]
        public double ProgressPercentage => TotalLectures > 0 
            ? Math.Round((double)CompletedLectures / TotalLectures * 100.0, 1) 
            : 0.0;
    }

    public class StudentProgressCardViewModel
    {
        public Student Student { get; set; } = new();
        public List<SubjectProgressItem> SubjectProgress { get; set; } = new();
        public int TotalLectures => SubjectProgress.Sum(s => s.TotalLectures);
        public int CompletedLectures => SubjectProgress.Sum(s => s.CompletedLectures);

        public double OverallProgressPercent
        {
            get
            {
                if (TotalLectures > 0)
                {
                    return Math.Round((double)CompletedLectures / TotalLectures * 100.0, 1);
                }
                return Student.ProgressPercentage;
            }
        }

        public bool CanPromote => OverallProgressPercent >= 100.0;

        public string? NextClassLevel
        {
            get
            {
                if (int.TryParse(Student.ClassLevel?.Replace("Class", "", StringComparison.OrdinalIgnoreCase).Trim(), out int current))
                {
                    return (current + 1).ToString();
                }
                return null;
            }
        }

        public string StatusDisplay
        {
            get
            {
                var st = Student.Status?.ToLower();
                if (st == "active" || st == "approved" || st == "verified") return "verified";
                if (st == "declined") return "declined";
                return "pending";
            }
        }
    }

    public class StudentProgressViewModel
    {
        public List<StudentProgressCardViewModel> Cards { get; set; } = new();
        public List<Student> Students { get; set; } = new();
        public int TotalStudents => Cards.Count;
        public double AvgProgress => Cards.Any() ? Math.Round(Cards.Average(c => c.OverallProgressPercent), 1) : 0.0;
        public int AvgAttendance => Cards.Any() ? (int)Math.Round(Cards.Average(c => c.Student.AttendancePercentage)) : 0;
        public string? SearchQuery { get; set; }
        public string? SelectedClass { get; set; }
        public List<string> AvailableClasses { get; set; } = new();
    }

    public class UpdateStudentProgressInput
    {
        public int StudentId { get; set; }
        public int AttendancePercentage { get; set; } = 90;
        public string? Notes { get; set; }
        public List<SubjectLectureProgressInput> SubjectProgress { get; set; } = new();
    }

    public class SubjectLectureProgressInput
    {
        public string SubjectName { get; set; } = string.Empty;
        public int TotalLectures { get; set; } = 12;
        public int CompletedLectures { get; set; } = 0;
    }
}
