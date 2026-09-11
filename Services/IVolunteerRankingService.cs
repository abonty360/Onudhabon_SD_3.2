using Onudhabon.Models;

namespace Onudhabon.Services
{
    public interface IVolunteerRankingService
    {
        VolunteerRankingViewModel GenerateMonthlyRankings(
            List<User> users,
            List<Lecture> lectures,
            List<Material> materials,
            List<Student> students,
            int month,
            int year,
            decimal eligibleDonationAmount = 0m,
            RankingConfiguration? configuration = null);
    }
}
