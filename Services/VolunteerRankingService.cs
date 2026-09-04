using Onudhabon_ISD.Models;

namespace Onudhabon_ISD.Services
{
    public class VolunteerRankingService : IVolunteerRankingService
    {
        public VolunteerRankingViewModel GenerateMonthlyRankings(
            List<User> users,
            List<Lecture> lectures,
            List<Material> materials,
            List<Student> students,
            int month,
            int year,
            decimal eligibleDonationAmount = 0m,
            RankingConfiguration? configuration = null)
        {
            var config = configuration ?? new RankingConfiguration();

            var isMonthCompleted = (year < DateTime.UtcNow.Year) ||
                                   (year == DateTime.UtcNow.Year && month < DateTime.UtcNow.Month);

            // 1. Educator Ranking Calculation
            var educators = users
                .Where(u => u.Role == "Educator")
                .ToList();

            var educatorRankingItems = new List<EducatorRankingItem>();

            foreach (var educator in educators)
            {
                // Active/Approved monthly lectures uploaded by this educator
                var monthlyLectures = lectures.Count(l =>
                    (l.Status == "Active" || l.Status == "Approved") &&
                    l.CreatedAt.Month == month &&
                    l.CreatedAt.Year == year &&
                    !string.IsNullOrEmpty(l.Instructor) &&
                    (l.Instructor.Equals(educator.FullName, StringComparison.OrdinalIgnoreCase) ||
                     l.Instructor.Equals(educator.Email, StringComparison.OrdinalIgnoreCase)));

                // Active/Approved monthly study materials uploaded by this educator
                var monthlyMaterials = materials.Count(m =>
                    (m.Status == "Active" || m.Status == "Approved") &&
                    m.Date.Month == month &&
                    m.Date.Year == year &&
                    !string.IsNullOrEmpty(m.Instructor) &&
                    (m.Instructor.Equals(educator.FullName, StringComparison.OrdinalIgnoreCase) ||
                     m.Instructor.Equals(educator.Email, StringComparison.OrdinalIgnoreCase)));

                educatorRankingItems.Add(new EducatorRankingItem
                {
                    User = educator,
                    MonthlyLecturesCount = monthlyLectures,
                    MonthlyMaterialsCount = monthlyMaterials
                });
            }

            // Sort educators by performance score (Monthly Lectures + Monthly Study Materials) descending
            var sortedEducators = educatorRankingItems
                .OrderByDescending(e => e.PerformanceScore)
                .ThenByDescending(e => e.MonthlyLecturesCount)
                .ThenByDescending(e => e.MonthlyMaterialsCount)
                .ThenBy(e => e.User.FullName)
                .ToList();

            for (int i = 0; i < sortedEducators.Count; i++)
            {
                int rank = i + 1;
                sortedEducators[i].Rank = rank;
                sortedEducators[i].RewardAmount = isMonthCompleted ? config.GetRewardAmount(rank) : 0m;
                sortedEducators[i].IsRewardEligible = isMonthCompleted && rank <= 3;
            }

            // 2. Local Guardian Ranking Calculation
            var guardians = users
                .Where(u => u.Role == "Local Guardian")
                .ToList();

            var guardianRankingItems = new List<GuardianRankingItem>();

            foreach (var guardian in guardians)
            {
                var guardianStudents = students.Where(s =>
                    (!string.IsNullOrEmpty(s.GuardianId) && (s.GuardianId == guardian.Id.ToString() || s.GuardianId.Equals(guardian.Email, StringComparison.OrdinalIgnoreCase))) ||
                    (!string.IsNullOrEmpty(s.GuardianName) && (s.GuardianName.Equals(guardian.FullName, StringComparison.OrdinalIgnoreCase) || s.GuardianName.Equals(guardian.Email, StringComparison.OrdinalIgnoreCase)))).ToList();

                int totalStudents = guardianStudents.Count;
                int activeStudents = guardianStudents.Count(s => s.Status == "Active" || s.Status == "Approved");
                double avgProgress = totalStudents > 0 ? Math.Round(guardianStudents.Average(s => s.ProgressPercentage), 1) : 0.0;

                // Configurable Score Formula:
                // Factor 1: Student Enrollment (scaled against benchmark, capped at 100)
                // Factor 2: Average Student Progress Percentage (0 - 100)
                double enrollmentComponent = config.GuardianEnrollmentBenchmark > 0
                    ? Math.Min(100.0, (totalStudents / (double)config.GuardianEnrollmentBenchmark) * 100.0)
                    : totalStudents;

                double performanceScore = Math.Round(
                    (enrollmentComponent * config.GuardianEnrollmentWeight) +
                    (avgProgress * config.GuardianProgressWeight), 1);

                guardianRankingItems.Add(new GuardianRankingItem
                {
                    User = guardian,
                    TotalAssignedStudents = totalStudents,
                    ActiveStudentsCount = activeStudents,
                    AverageStudentProgress = avgProgress,
                    PerformanceScore = performanceScore
                });
            }

            // Sort local guardians by performance score descending
            var sortedGuardians = guardianRankingItems
                .OrderByDescending(g => g.PerformanceScore)
                .ThenByDescending(g => g.AverageStudentProgress)
                .ThenByDescending(g => g.TotalAssignedStudents)
                .ThenBy(g => g.User.FullName)
                .ToList();

            for (int i = 0; i < sortedGuardians.Count; i++)
            {
                int rank = i + 1;
                sortedGuardians[i].Rank = rank;
                sortedGuardians[i].RewardAmount = isMonthCompleted ? config.GetRewardAmount(rank) : 0m;
                sortedGuardians[i].IsRewardEligible = isMonthCompleted && rank <= 3;
            }

            return new VolunteerRankingViewModel
            {
                SelectedMonth = month,
                SelectedYear = year,
                EligibleDonationAmount = eligibleDonationAmount,
                Configuration = config,
                EducatorRankings = sortedEducators,
                GuardianRankings = sortedGuardians
            };
        }
    }
}
