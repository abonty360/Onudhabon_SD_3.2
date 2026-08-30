namespace Onudhabon_ISD.Models
{
    public class AdminDashboardViewModel
    {
        public List<User> Users { get; set; } = new();
        public List<Lecture> Lectures { get; set; } = new();
        public List<Material> Materials { get; set; } = new();
        public List<ForumPost> ForumPosts { get; set; } = new();
        public List<Student> Students { get; set; } = new();

        public int TotalUsers => Users.Count;
        public int PendingVolunteersCount => Users.Count(u => u.Role != "Admin" && (u.VerificationStatus == "Pending" || string.IsNullOrEmpty(u.VerificationStatus)));
        public int ActiveVolunteersCount => Users.Count(u => u.VerificationStatus == "Active" || u.VerificationStatus == "Approved");
        public int DeclinedVolunteersCount => Users.Count(u => u.VerificationStatus == "Declined");
        public int RestrictedUsersCount => Users.Count(u => u.IsRestricted);

        public int PendingLecturesCount => Lectures.Count(l => l.Status == "pending" || l.Status == "Pending");
        public int ApprovedLecturesCount => Lectures.Count(l => l.Status == "Active" || l.Status == "Approved");

        public int PendingMaterialsCount => Materials.Count(m => m.Status == "pending" || m.Status == "Pending");
        public int ApprovedMaterialsCount => Materials.Count(m => m.Status == "Active" || m.Status == "Approved");

        public int PendingForumPostsCount => ForumPosts.Count(f => f.Status == "pending" || f.Status == "Pending" || string.IsNullOrEmpty(f.Status));
        public int ApprovedForumPostsCount => ForumPosts.Count(f => f.Status == "Active" || f.Status == "Approved");

        public int PendingStudentsCount => Students.Count(s => s.Status == "pending" || s.Status == "Pending" || string.IsNullOrEmpty(s.Status));
        public int ApprovedStudentsCount => Students.Count(s => s.Status == "Active" || s.Status == "Approved");
        public int TotalStudentsCount => Students.Count;
    }
}
