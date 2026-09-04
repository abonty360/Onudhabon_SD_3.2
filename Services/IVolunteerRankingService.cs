using Onudhabon_ISD.Models;

namespace Onudhabon_ISD.Services
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
