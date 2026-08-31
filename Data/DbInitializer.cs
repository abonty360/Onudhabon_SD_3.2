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
                        Status = "Active",
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
                        Status = "Active",
                        Likes = 1,
                        Dislikes = 0,
                        Replies = 0,
                        CreatedAt = new DateTime(2025, 9, 14, 9, 15, 20, DateTimeKind.Utc)
                    }
                };

                context.ForumPosts.AddRange(samplePosts);
                context.SaveChanges();
            }
            else
            {
                var postsNeedingStatus = context.ForumPosts.Where(p => p.Status == null || p.Status == "").ToList();
                if (postsNeedingStatus.Any())
                {
                    foreach (var p in postsNeedingStatus)
                    {
                        p.Status = "Active";
                    }
                    context.SaveChanges();
                }
            }
        }

        public static void SeedDonations(ApplicationDbContext context)
        {
            if (!context.Donations.Any())
            {
                var sampleDonations = new List<Donation>
                {
                    new Donation
                    {
                        DonorName = "Rahim Chowdhury",
                        DonorEmail = "rahim.chowdhury@example.com",
                        DonorPhone = "01711223344",
                        Amount = 2500.00m,
                        Currency = "BDT",
                        Purpose = "Student Learning Materials & Textbooks",
                        Message = "Keep up the noble mission for underprivileged kids education!",
                        PaymentMethod = "bKash",
                        BkashWalletNumber = "01711223344",
                        BkashTransactionId = "TRX9A8B7C6D",
                        BkashPaymentId = "BK-20260828104522-4912",
                        Status = "Completed",
                        IsAnonymous = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        __v = 0
                    },
                    new Donation
                    {
                        DonorName = "Dr. Nusrat Jahan",
                        DonorEmail = "nusrat.jahan@example.com",
                        DonorPhone = "01819345678",
                        Amount = 5000.00m,
                        Currency = "BDT",
                        Purpose = "Community Digital Classrooms",
                        Message = "Supporting digital literacy and recorded video lecture equipment.",
                        PaymentMethod = "bKash",
                        BkashWalletNumber = "01819345678",
                        BkashTransactionId = "TRX8K7L6M5N",
                        BkashPaymentId = "BK-20260829142010-8201",
                        Status = "Completed",
                        IsAnonymous = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        __v = 0
                    },
                    new Donation
                    {
                        DonorName = "Anonymous Well-Wisher",
                        DonorEmail = "donor@gmail.com",
                        DonorPhone = "01912987654",
                        Amount = 1000.00m,
                        Currency = "BDT",
                        Purpose = "General Education & Child Support Fund",
                        Message = "In memory of my parents.",
                        PaymentMethod = "bKash",
                        BkashWalletNumber = "01912987654",
                        BkashTransactionId = "TRX7P6Q5R4S",
                        BkashPaymentId = "BK-20260830091530-1094",
                        Status = "Completed",
                        IsAnonymous = true,
                        CreatedAt = DateTime.UtcNow.AddHours(-18),
                        __v = 0
                    }
                };

                context.Donations.AddRange(sampleDonations);
                context.SaveChanges();
            }
        }

        public static void SeedStudents(ApplicationDbContext context)
        {
            if (!context.Students.Any())
            {
                var students = new List<Student>
                {
                    new Student
                    {
                        FullName = "New Student",
                        BirthCertificateId = "2694262689036",
                        Address = "Sreepur, Gazipur",
                        FatherName = "John Doe",
                        MotherName = "Jane Doe",
                        ClassLevel = "5",
                        EnrollmentYear = "2025",
                        GuardianName = "Local Guardian",
                        GuardianId = "localguardian@example.com",
                        Status = "Active",
                        CompletedClasses = 4,
                        Age = 11,
                        AttendancePercentage = 95,
                        ProgressPercentage = 14,
                        Notes = "Active student showing great aptitude in Bangla 2nd paper and Social Science.",
                        Subjects = "Bangla 1st paper, Bangla 2nd paper, English 1st paper, English 2nd paper, Math, Social Science, General Science",
                        SubjectProgressJson = "[{\"subjectName\":\"Bangla 1st paper\",\"totalLectures\":12,\"completedLectures\":0},{\"subjectName\":\"Bangla 2nd paper\",\"totalLectures\":12,\"completedLectures\":6},{\"subjectName\":\"English 1st paper\",\"totalLectures\":12,\"completedLectures\":0},{\"subjectName\":\"English 2nd paper\",\"totalLectures\":12,\"completedLectures\":0},{\"subjectName\":\"Math\",\"totalLectures\":12,\"completedLectures\":0},{\"subjectName\":\"Social Science\",\"totalLectures\":12,\"completedLectures\":6},{\"subjectName\":\"General Science\",\"totalLectures\":12,\"completedLectures\":0}]",
                        CreatedAt = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc),
                        LastActivityDate = DateTime.UtcNow.AddDays(-1),
                        __v = 0
                    },
                    new Student
                    {
                        FullName = "Janet Doe",
                        BirthCertificateId = "1307259",
                        Address = "Mohakhali, Dhaka",
                        FatherName = "John Doe",
                        MotherName = "Mary Doe",
                        ClassLevel = "11",
                        EnrollmentYear = "2025",
                        GuardianName = "Local Guardian",
                        GuardianId = "localguardian@example.com",
                        Status = "Active",
                        CompletedClasses = 10,
                        Age = 17,
                        AttendancePercentage = 92,
                        ProgressPercentage = 8,
                        Notes = "Focused on HSC preparation. Excelling in Bangla papers.",
                        Subjects = "Bangla 1st paper, Bangla 2nd paper, English 1st paper, English 2nd paper, Physics 1st paper, Physics 2nd paper, Chemistry 1st paper, Chemistry 2nd paper, Higher Math 1st paper, Higher Math 2nd paper, Biology 1st paper, Biology 2nd paper",
                        SubjectProgressJson = "[{\"subjectName\":\"Bangla 1st paper\",\"totalLectures\":20,\"completedLectures\":10},{\"subjectName\":\"Bangla 2nd paper\",\"totalLectures\":20,\"completedLectures\":10},{\"subjectName\":\"English 1st paper\",\"totalLectures\":20,\"completedLectures\":0},{\"subjectName\":\"English 2nd paper\",\"totalLectures\":20,\"completedLectures\":0},{\"subjectName\":\"Physics 1st paper\",\"totalLectures\":20,\"completedLectures\":0},{\"subjectName\":\"Physics 2nd paper\",\"totalLectures\":20,\"completedLectures\":0},{\"subjectName\":\"Chemistry 1st paper\",\"totalLectures\":20,\"completedLectures\":0},{\"subjectName\":\"Chemistry 2nd paper\",\"totalLectures\":20,\"completedLectures\":0},{\"subjectName\":\"Higher Math 1st paper\",\"totalLectures\":20,\"completedLectures\":0},{\"subjectName\":\"Higher Math 2nd paper\",\"totalLectures\":20,\"completedLectures\":0},{\"subjectName\":\"Biology 1st paper\",\"totalLectures\":20,\"completedLectures\":0},{\"subjectName\":\"Biology 2nd paper\",\"totalLectures\":20,\"completedLectures\":0}]",
                        CreatedAt = new DateTime(2025, 2, 10, 11, 30, 0, DateTimeKind.Utc),
                        LastActivityDate = DateTime.UtcNow.AddDays(-2),
                        __v = 0
                    },
                    new Student
                    {
                        FullName = "Student New",
                        BirthCertificateId = "2694262689038",
                        Address = "Mirpur, Dhaka",
                        FatherName = "John Doe",
                        MotherName = "Salma Khatun",
                        ClassLevel = "1",
                        EnrollmentYear = "2025",
                        GuardianName = "Local Guardian",
                        GuardianId = "localguardian@example.com",
                        Status = "Declined",
                        CompletedClasses = 0,
                        Age = 7,
                        AttendancePercentage = 80,
                        ProgressPercentage = 0,
                        Notes = "Application submitted for primary class enrollment.",
                        Subjects = "Bangla, English, Math",
                        SubjectProgressJson = "[{\"subjectName\":\"Bangla\",\"totalLectures\":10,\"completedLectures\":0},{\"subjectName\":\"English\",\"totalLectures\":10,\"completedLectures\":0},{\"subjectName\":\"Math\",\"totalLectures\":10,\"completedLectures\":0}]",
                        CreatedAt = new DateTime(2025, 3, 5, 9, 15, 0, DateTimeKind.Utc),
                        LastActivityDate = DateTime.UtcNow.AddDays(-5),
                        __v = 0
                    }
                };

                context.Students.AddRange(students);
                context.SaveChanges();
            }
        }
    }
}
