namespace Onudhabon.Models
{
    public class ForumAuthorDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Initial { get; set; } = "U";
        public bool IsVerified { get; set; }
        public string VerificationStatus { get; set; } = "Pending";
        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
    }
}
