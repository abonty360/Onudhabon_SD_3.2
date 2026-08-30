using Microsoft.AspNetCore.Identity;
using Onudhabon_ISD.Models;

namespace Onudhabon_ISD.Data
{
    public static class DbInitializer
    {
        public static void SeedAdminUser(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            const string adminEmail = "admin@onudhabon.com";

            var existingAdmin = context.Users.FirstOrDefault(u => u.Email.ToLower() == adminEmail.ToLower());
            if (existingAdmin == null)
            {
                var admin = new User
                {
                    FullName = "System Administrator",
                    Email = adminEmail,
                    PhoneNumber = "01700000000",
                    Role = "Admin",
                    City = "Dhaka",
                    Area = "Central",
                    Location = "Onudhabon Administrative HQ, Dhaka",
                    VolunteerReason = "Platform Administration and Volunteer Oversight",
                    Bio = "Primary System Administrator overseeing volunteer verification, content moderation, and platform governance.",
                    IsRestricted = false,
                    IsVerified = true,
                    VerificationStatus = "Active",
                    AgreeToTerms = true,
                    CreatedAt = DateTime.UtcNow,
                    __v = 0
                };

                admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin@12345");

                context.Users.Add(admin);
                context.SaveChanges();
            }
        }

        public static void SeedClassPlans(ApplicationDbContext context)
        {
            if (!context.ClassPlans.Any())
            {
                var plans = new List<ClassPlan>();

                // Classes 1–3: 3 subjects, 10 lectures each
                for (int level = 1; level <= 3; level++)
                {
                    plans.Add(new ClassPlan
                    {
                        ClassLevel = level.ToString(),
                        Subjects = new List<SubjectDetail>
                        {
                            new() { Name = "Bangla", TotalLectures = 10 },
                            new() { Name = "English", TotalLectures = 10 },
                            new() { Name = "Math", TotalLectures = 10 }
                        }
                    });
                }

                // Classes 4–8: 7 subjects, 12 lectures each
                for (int level = 4; level <= 8; level++)
                {
                    plans.Add(new ClassPlan
                    {
                        ClassLevel = level.ToString(),
                        Subjects = new List<SubjectDetail>
                        {
                            new() { Name = "Bangla 1st paper", TotalLectures = 12 },
                            new() { Name = "Bangla 2nd paper", TotalLectures = 12 },
                            new() { Name = "English 1st paper", TotalLectures = 12 },
                            new() { Name = "English 2nd paper", TotalLectures = 12 },
                            new() { Name = "Math", TotalLectures = 12 },
                            new() { Name = "Social Science", TotalLectures = 12 },
                            new() { Name = "General Science", TotalLectures = 12 }
                        }
                    });
                }

                // Classes 9–10: 11 subjects, 12 lectures each
                for (int level = 9; level <= 10; level++)
                {
                    plans.Add(new ClassPlan
                    {
                        ClassLevel = level.ToString(),
                        Subjects = new List<SubjectDetail>
                        {
                            new() { Name = "Bangla 1st paper", TotalLectures = 12 },
                            new() { Name = "Bangla 2nd paper", TotalLectures = 12 },
                            new() { Name = "English 1st paper", TotalLectures = 12 },
                            new() { Name = "English 2nd paper", TotalLectures = 12 },
                            new() { Name = "Math", TotalLectures = 12 },
                            new() { Name = "Social Science", TotalLectures = 12 },
                            new() { Name = "General Science", TotalLectures = 12 },
                            new() { Name = "Physics", TotalLectures = 12 },
                            new() { Name = "Chemistry", TotalLectures = 12 },
                            new() { Name = "Higher Math", TotalLectures = 12 },
                            new() { Name = "Biology", TotalLectures = 12 }
                        }
                    });
                }

                // Classes 11–12: 12 subjects, 20 lectures each
                for (int level = 11; level <= 12; level++)
                {
                    plans.Add(new ClassPlan
                    {
                        ClassLevel = level.ToString(),
                        Subjects = new List<SubjectDetail>
                        {
                            new() { Name = "Bangla 1st paper", TotalLectures = 20 },
                            new() { Name = "Bangla 2nd paper", TotalLectures = 20 },
                            new() { Name = "English 1st paper", TotalLectures = 20 },
                            new() { Name = "English 2nd paper", TotalLectures = 20 },
                            new() { Name = "Physics 1st paper", TotalLectures = 20 },
                            new() { Name = "Physics 2nd paper", TotalLectures = 20 },
                            new() { Name = "Chemistry 1st paper", TotalLectures = 20 },
                            new() { Name = "Chemistry 2nd paper", TotalLectures = 20 },
                            new() { Name = "Higher Math 1st paper", TotalLectures = 20 },
                            new() { Name = "Higher Math 2nd paper", TotalLectures = 20 },
                            new() { Name = "Biology 1st paper", TotalLectures = 20 },
                            new() { Name = "Biology 2nd paper", TotalLectures = 20 }
                        }
                    });
                }

                context.ClassPlans.AddRange(plans);
                context.SaveChanges();
            }
        }

        public static void SeedForumPosts(ApplicationDbContext context)
        {
            if (!context.ForumPosts.Any())
            {
                var samplePosts = new List<ForumPost>
                {
                    new ForumPost
                    {
                        Title = "Physics Lecture",
                        Content = "Are there any Educators planning to create lectures on 11th grade physics -> Electricity topic? If not, then I'd like to...",
                        Author = "Abonty Rahman Flora",
                        AuthorRole = "Educator",
                        Tags = "#discussion #lectures",
                        Category = "General",
                        Likes = 5,
                        Dislikes = 0,
                        Replies = 1,
                        CreatedAt = new DateTime(2025, 9, 19, 19, 23, 13, DateTimeKind.Utc)
                    },
                    new ForumPost
                    {
                        Title = "Student Progress Tracker Live",
                        Content = "Local Guardians can now track live progress of their enrolled students....",
                        Author = "System Admin",
                        AuthorRole = "Admin",
                        Tags = "#localguardian #studentprogress",
                        Category = "General",
                        Likes = 1,
                        Dislikes = 0,
                        Replies = 0,
                        CreatedAt = new DateTime(2025, 9, 14, 9, 15, 20, DateTimeKind.Utc)
                    }
                };

                context.ForumPosts.AddRange(samplePosts);
                context.SaveChanges();
            }
        }
    }
}
