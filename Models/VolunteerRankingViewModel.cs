namespace Onudhabon_ISD.Models
{
    public class RankingConfiguration
    {
        // Configurable weights for Local Guardian performance score (sum = 1.0)
        public double GuardianEnrollmentWeight { get; set; } = 0.40; // 40% weight
        public double GuardianProgressWeight { get; set; } = 0.60;   // 60% weight

        // Base max benchmark for student enrollment scaling in score calculation (10 students = 100% on enrollment metric)
        public int GuardianEnrollmentBenchmark { get; set; } = 10;

        // Hardcoded Monthly Reward Amounts for Top 3 Positions (in BDT)
        public decimal FirstPlaceRewardAmount { get; set; } = 10000m; // 10k (৳10,000)
        public decimal SecondPlaceRewardAmount { get; set; } = 7000m;  // 7k (৳7,000)
        public decimal ThirdPlaceRewardAmount { get; set; } = 4000m;   // 4k (৳4,000)

        public decimal GetRewardAmount(int rank)
        {
            return rank switch
            {
                1 => FirstPlaceRewardAmount,
                2 => SecondPlaceRewardAmount,
                3 => ThirdPlaceRewardAmount,
                _ => 0m
            };
        }

        public string GetRewardLabel(int rank)
        {
            return rank switch
            {
                1 => "10k (৳10,000)",
                2 => "7k (৳7,000)",
                3 => "4k (৳4,000)",
                _ => "—"
            };
        }
    }

    public class EducatorRankingItem
    {
        public int Rank { get; set; }
        public User User { get; set; } = null!;
        public int MonthlyLecturesCount { get; set; }
        public int MonthlyMaterialsCount { get; set; }
        public int TotalMonthlyUploads => MonthlyLecturesCount + MonthlyMaterialsCount;
        public double PerformanceScore => TotalMonthlyUploads;

        // Monthly Reward properties (Assigned only upon month completion: 1st: 10k, 2nd: 7k, 3rd: 4k)
        public decimal RewardAmount { get; set; }
        public decimal CalculatedRewardAmount => RewardAmount;
        public bool IsRewardEligible { get; set; }
        public string RewardLabel => RewardAmount > 0
            ? (RewardAmount == 10000m ? "10k (৳10,000)" : RewardAmount == 7000m ? "7k (৳7,000)" : RewardAmount == 4000m ? "4k (৳4,000)" : $"৳{RewardAmount:N0}")
            : "—";
    }

    public class GuardianRankingItem
    {
        public int Rank { get; set; }
        public User User { get; set; } = null!;
        public int TotalAssignedStudents { get; set; }
        public int ActiveStudentsCount { get; set; }
        public double AverageStudentProgress { get; set; }
        public double PerformanceScore { get; set; }

        // Monthly Reward properties (Assigned only upon month completion: 1st: 10k, 2nd: 7k, 3rd: 4k)
        public decimal RewardAmount { get; set; }
        public decimal CalculatedRewardAmount => RewardAmount;
        public bool IsRewardEligible { get; set; }
        public string RewardLabel => RewardAmount > 0
            ? (RewardAmount == 10000m ? "10k (৳10,000)" : RewardAmount == 7000m ? "7k (৳7,000)" : RewardAmount == 4000m ? "4k (৳4,000)" : $"৳{RewardAmount:N0}")
            : "—";
    }

    public class VolunteerRankingViewModel
    {
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }

        // Identifies whether the selected month is past/completed vs currently live/ongoing
        public bool IsMonthCompleted =>
            (SelectedYear < DateTime.UtcNow.Year) ||
            (SelectedYear == DateTime.UtcNow.Year && SelectedMonth < DateTime.UtcNow.Month);

        public decimal EligibleDonationAmount { get; set; } = 0m;

        public RankingConfiguration Configuration { get; set; } = new();
        public List<EducatorRankingItem> EducatorRankings { get; set; } = new();
        public List<GuardianRankingItem> GuardianRankings { get; set; } = new();
    }
}
