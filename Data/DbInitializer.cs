using Onudhabon_ISD.Models;

namespace Onudhabon_ISD.Data
{
    public static class DbInitializer
    {
        public static void SeedClassPlans(ApplicationDbContext context)
        {
            if (!context.ClassPlans.Any())
            {
                var plans = new List<ClassPlan>();

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
    }
}