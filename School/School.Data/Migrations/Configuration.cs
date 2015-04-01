namespace School.Data.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using School.Common;
    using School.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<School.Data.ApplicationDbContext>
    {
        private const int lastSchoolYear = 12;

        private const int academicYearsCount = 3;

        private DateTime startDate = new DateTime(2012, 9, 15);
        private DateTime endDate = new DateTime(2013, 5, 31);

        private const int classStudentsNumber = 20;
        private const int gradeClassesNumber = 5;

        private int studentCounter = 1;

        private int teacherCounter = 1;

        private readonly List<string> personNames = new List<string>()
        {
            "Teddy Ferrara",
            "Dyan Fisher",
            "Anne Smith",
            "Maria Finnegan",
            "Ronnie Foltz",
            "Eleanor Fowler",
            "William Heller",
            "Bobbi Canfield",
            "Christina Buxton",
            "Alexander Byrnes",
            "Simon Cambell",
            "Peter Callaghan",
            "Ashley Hong",
            "Hayden Jacques",
            "Ida Jacobson",
            "Jamie Miller",
            "Jason Peterson",
            "Michael Kaiser",
            "Ivy Kearney",
            "Sammy Keen",
        };
        
        private readonly List<string> generalSchoolThemeSubjectNames = new List<string>()
        {
            "Literature",
            "Languages",
            "Mathematics",
            "Computer Science",
            "Arts",
            "Music",
            "Physical education"
        };
        
        private readonly List<string> schoolThemeNames = new List<string>()
        {
            "Science, Technology, Engineering, Math (STEM)",
            "Medical Careers (MC)",
            "Humanities (H)",
            "General"
        };


        public Configuration()
        {
            this.AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = true;
        }

        private UserManager<ApplicationUser> userManager;

        private RoleManager<IdentityRole> roleManager;

        protected override void Seed(School.Data.ApplicationDbContext context)
        {
            userManager = this.CreateUserManager(context);
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            context.Configuration.AutoDetectChangesEnabled = false;

            this.SeedRoles(context);
            this.SeedAdministrators(context);
            this.SeedAcademicYears(context, academicYearsCount);

            context.Configuration.AutoDetectChangesEnabled = true;
        }

        private void SeedRoles(ApplicationDbContext context)
        {
            if (context.Roles.Any())
            {
                return;
            }

            context.Roles.AddOrUpdate(new IdentityRole(GlobalConstants.SuperAdministratorRoleName));
            context.Roles.AddOrUpdate(new IdentityRole(GlobalConstants.AdministratorRoleName));
            context.Roles.AddOrUpdate(new IdentityRole(GlobalConstants.TeacherRoleName));
            context.Roles.AddOrUpdate(new IdentityRole(GlobalConstants.StudentRoleName));

            context.SaveChanges();
        }

        private void SeedAdministrators(ApplicationDbContext context)
        {
            if (context.Administrators.Any())
            {
                return;
            }

            var adminProfile = new Administrator();
            adminProfile.FirstName = "SuperAdmin";
            adminProfile.LastName = "SuperAdmin";

            var adminUser = new ApplicationUser();
            adminUser.UserName = "superadmin";
            adminUser.Email = "superadmin@superadmin.com";

            string password = "111";

            SeedAdminApplicationUser(context, adminUser, password);

            adminProfile.ApplicationUser = adminUser;

            context.Administrators.Add(adminProfile);

            adminProfile = new Administrator();
            adminProfile.FirstName = "Admin";
            adminProfile.LastName = "Admin";

            adminUser = new ApplicationUser();
            adminUser.UserName = "admin";
            adminUser.Email = "admin@admin.com";

            SeedAdminApplicationUser(context, adminUser, password);

            adminProfile.ApplicationUser = adminUser;

            context.Administrators.Add(adminProfile);

            context.SaveChanges();
        }

        private void SeedAdminApplicationUser(ApplicationDbContext context, ApplicationUser adminUser, string password)
        {
            if (!roleManager.RoleExists(GlobalConstants.SuperAdministratorRoleName))
            {
                roleManager.Create(new IdentityRole(GlobalConstants.SuperAdministratorRoleName));
            }

            if (!roleManager.RoleExists(GlobalConstants.AdministratorRoleName))
            {
                roleManager.Create(new IdentityRole(GlobalConstants.AdministratorRoleName));
            }

            var result = userManager.Create(adminUser, password);

            if (result.Succeeded)
            {
                userManager.AddToRole(adminUser.Id, GlobalConstants.AdministratorRoleName);

                if (adminUser.UserName == "superadmin")
                {
                    userManager.AddToRole(adminUser.Id, GlobalConstants.SuperAdministratorRoleName);
                }
            }
        }

        private void SeedAcademicYears(ApplicationDbContext context, int academicYearsCount)
        {
            var academicYears = new List<AcademicYear>();

            if (context.AcademicYears.Any())
            {
                return;
            }

            var previousAcademicYear = new AcademicYear();

            previousAcademicYear.StartDate = startDate;
            previousAcademicYear.EndDate = endDate;

            for (int i = 0; i < academicYearsCount; i++)
            {
                if (academicYears.Count() > 0)
                {
                    previousAcademicYear = academicYears.Last();
                }

                var academicYear = SeedSingleAcademicYear(context, previousAcademicYear, lastSchoolYear, gradeClassesNumber);
                academicYears.Add(academicYear);
            }
        }

        private AcademicYear SeedSingleAcademicYear(
            ApplicationDbContext context,
            AcademicYear previousAcademicYear,
            int lastSchoolYear,
            int gradeClassesNumber)
        {
            var academicYear = new AcademicYear();

            if (previousAcademicYear.Grades.Count() > 0)
            {
                academicYear.StartDate = previousAcademicYear.StartDate.AddYears(1);
                academicYear.EndDate = previousAcademicYear.EndDate.AddYears(1);
            }
            else
            {
                academicYear.StartDate = previousAcademicYear.StartDate;
                academicYear.EndDate = previousAcademicYear.EndDate;
            }

            academicYear.IsActive = true;
            previousAcademicYear.IsActive = false;

            IList<SchoolTheme> previousAcademicYearSchoolThemes = new List<SchoolTheme>();

            if (previousAcademicYear.Grades.Count() > 0 && previousAcademicYear.SchoolThemes.Count() > 0)
            {
                previousAcademicYearSchoolThemes = previousAcademicYear.SchoolThemes;

            }

            IList<SchoolTheme> schoolThemes = SeedSchoolThemes(context, previousAcademicYearSchoolThemes, academicYear);

            academicYear.SchoolThemes = schoolThemes;

            IList<Grade> previousAcademicYearGrades = new List<Grade>();
            int previousAcademicYearNumber = 0;

            if (previousAcademicYear.Grades.Count() > 0)
            {
                previousAcademicYearGrades = previousAcademicYear.Grades;
                previousAcademicYearNumber = previousAcademicYear.StartDate.Year;
            }

            academicYear.Grades = SeedGrades(context, previousAcademicYearGrades, lastSchoolYear, academicYear);

            foreach (var grade in academicYear.Grades)
            {
                IList<Subject> previousYearCurrentGradeSubjects = new List<Subject>();

                if (previousAcademicYearGrades.Count() > 0)
                {
                    previousYearCurrentGradeSubjects = previousAcademicYearGrades
                        .FirstOrDefault(g => g.GradeYear == grade.GradeYear)
                        .Subjects;
                }

                SeedGradeSubjects(context, schoolThemes, grade, previousYearCurrentGradeSubjects);

                IList<SchoolClass> previousYearCurrentGradeClasses = new List<SchoolClass>();

                if (previousAcademicYearGrades.Count() > 0)
                {
                    previousYearCurrentGradeClasses = previousAcademicYearGrades
                        .FirstOrDefault(g => g.GradeYear == grade.GradeYear)
                        .SchoolClasses;
                }

                if (previousAcademicYear.Grades.Count() > 0 && grade.GradeYear > 1)
                {
                    gradeClassesNumber = previousAcademicYear
                        .Grades
                        .First(g => g.GradeYear == grade.GradeYear - 1)
                        .SchoolClasses
                        .Count();
                }

                SeedGradeSchoolClasses(context, grade, previousYearCurrentGradeClasses, gradeClassesNumber, schoolThemes);
                
                foreach (var schoolClass in grade.SchoolClasses)
                {
                    SchoolClass oldSchoolClass = new SchoolClass();

                    if (previousAcademicYear.Grades.Count() > 0 && grade.GradeYear > 1)
                    {
                        oldSchoolClass = previousAcademicYear
                           .Grades
                           .FirstOrDefault(g => g.GradeYear == grade.GradeYear - 1)
                           .SchoolClasses
                           .FirstOrDefault(sc => sc.ClassLetter == schoolClass.ClassLetter);
                    }

                    schoolClass.Students = SeedSchoolClassStudents(context, oldSchoolClass, classStudentsNumber);
                }
            }

            SeedTeachers(context, academicYear.Grades, previousAcademicYearGrades);

            context.AcademicYears.AddOrUpdate(academicYear);
            context.SaveChanges();

            return academicYear;
        }

        private IList<SchoolTheme> SeedSchoolThemes(
            ApplicationDbContext context,
            IList<SchoolTheme> previousAcademicYearSchoolThemes,
            AcademicYear currentAcademicYear)
        {
            IList<SchoolTheme> schoolThemes = new List<SchoolTheme>();

            if (previousAcademicYearSchoolThemes.Count() > 0)
            {
                foreach (var schoolTheme in previousAcademicYearSchoolThemes)
                {
                    schoolTheme.AcademicYears.Add(currentAcademicYear);
                    context.SchoolThemes.AddOrUpdate(schoolTheme);
                }

                schoolThemes = previousAcademicYearSchoolThemes;
            }
            else
            {
                foreach (var schoolThemeName in schoolThemeNames)
                {
                    SchoolTheme schoolTheme = new SchoolTheme();
                    schoolTheme.Name = schoolThemeName;

                    if (schoolThemeName == "General")
                    {
                        schoolTheme.StartGradeYear = 1;
                        schoolTheme.EndGradeYear = 7;
                    }
                    else
                    {
                        schoolTheme.StartGradeYear = 8;
                        schoolTheme.EndGradeYear = 12;
                    }

                    schoolTheme.AcademicYears.Add(currentAcademicYear);

                    context.SchoolThemes.AddOrUpdate(schoolTheme);
                    schoolThemes.Add(schoolTheme);
                }
            }

            return schoolThemes;
        }

        private List<Grade> SeedGrades(
            ApplicationDbContext context,
            IList<Grade> previousAcademicYearGrades,
            int lastSchoolYear,
            AcademicYear currentAcademicYear)
        {
            var grades = new List<Grade>();

            if (previousAcademicYearGrades == null || previousAcademicYearGrades.Count() == 0)
            {
                for (int gradeIndex = 0; gradeIndex < lastSchoolYear; gradeIndex++)
                {
                    Grade grade = new Grade();
                    grade.GradeYear = gradeIndex + 1;
                    grade.AcademicYear = currentAcademicYear;

                    context.Grades.AddOrUpdate(grade);
                    grades.Add(grade);
                }
            }
            else
            {
                Grade grade = new Grade();
                grade.GradeYear = 1;
                grade.AcademicYear = currentAcademicYear;

                context.Grades.AddOrUpdate(grade);
                grades.Add(grade);

                foreach (var previousAcademicYearGrade in previousAcademicYearGrades)
                {
                    if (previousAcademicYearGrade.GradeYear < lastSchoolYear)
                    {
                        grade = new Grade();
                        grade.GradeYear = previousAcademicYearGrade.GradeYear + 1;
                        grade.AcademicYear = currentAcademicYear;

                        context.Grades.AddOrUpdate(grade);
                        grades.Add(grade);
                    }
                }
            }

            return grades;
        }

        private List<Subject> SeedGradeSubjects(
            ApplicationDbContext context,
            IList<SchoolTheme> schoolThemes,
            Grade grade,
            IList<Subject> previousYearCurrentGradeSubjects)
        {
            List<Subject> subjects = new List<Subject>();

            if (previousYearCurrentGradeSubjects != null && previousYearCurrentGradeSubjects.Count() > 0)
            {
                //Copies subject information from previous year current grade to the new subjects
                foreach (var previousYearCurrentGradeSubject in previousYearCurrentGradeSubjects)
                {
                    Subject subject = new Subject();
                    subject.Name = previousYearCurrentGradeSubject.Name;
                    subject.Grade = grade;
                    subject.TotalHours = previousYearCurrentGradeSubject.TotalHours;
                    subject.SchoolTheme = previousYearCurrentGradeSubject.SchoolTheme;

                    context.Subjects.AddOrUpdate(subject);
                    subjects.Add(subject);
                }
            }
            else
            {
                if (grade.GradeYear < 8)
                {
                    SchoolTheme generalSchoolTheme = schoolThemes.FirstOrDefault(st => st.Name == "General");
                    subjects = SeedPrimarySchoolGradeSubjects(context, generalSchoolTheme, grade);
                }
                else
                {
                    IList<SchoolTheme> schoolThemesWithoutGeneral = schoolThemes.Where(st => st.Name != "General").ToList();
                    subjects = SeedSecondarySchoolGradeSubjects(context, schoolThemesWithoutGeneral, grade);
                }
            }

            return subjects;
        }

        private List<Subject> SeedPrimarySchoolGradeSubjects(
            ApplicationDbContext context,
            SchoolTheme generalSchoolTheme,
            Grade grade)
        {
            List<Subject> subjects = new List<Subject>();

            foreach (var subjectName in generalSchoolThemeSubjectNames)
            {
                Subject subject = new Subject();
                subject.Name = subjectName;
                subject.Grade = grade;
                subject.TotalHours = 80;
                subject.SchoolTheme = generalSchoolTheme;
                context.Subjects.AddOrUpdate(subject);
                subjects.Add(subject);
            }

            return subjects;
        }

        private List<Subject> SeedSecondarySchoolGradeSubjects(
            ApplicationDbContext context,
            IList<SchoolTheme> schoolThemes,
            Grade grade)
        {
            List<Subject> subjects = new List<Subject>();

            foreach (var schoolTheme in schoolThemes)
            {
                if (schoolTheme.Name == "Science, Technology, Engineering, Math (STEM)")
                {
                    Subject subject = new Subject();
                    subject.Name = "Physics";
                    subject.TotalHours = 110;
                    subject.Grade = grade;
                    subject.SchoolTheme = schoolTheme;

                    context.Subjects.AddOrUpdate(subject);
                    subjects.Add(subject);

                    subject = new Subject();
                    subject.Name = "Mathematics";
                    subject.TotalHours = 90;
                    subject.Grade = grade;
                    subject.SchoolTheme = schoolTheme;

                    context.Subjects.AddOrUpdate(subject);
                    subjects.Add(subject);

                    subject = new Subject();
                    subject.Name = "Chemistry";
                    subject.TotalHours = 70;
                    subject.Grade = grade;
                    subject.SchoolTheme = schoolTheme;

                    context.Subjects.AddOrUpdate(subject);
                    subjects.Add(subject);
                }

                if (schoolTheme.Name == "Medical Careers (MC)")
                {
                    Subject subject = new Subject();
                    subject.Name = "Biology";
                    subject.TotalHours = 120;
                    subject.Grade = grade;
                    subject.SchoolTheme = schoolTheme;

                    context.Subjects.AddOrUpdate(subject);
                    subjects.Add(subject);

                    subject = new Subject();
                    subject.Name = "Chemistry";
                    subject.TotalHours = 100;
                    subject.Grade = grade;
                    subject.SchoolTheme = schoolTheme;

                    context.Subjects.AddOrUpdate(subject);
                    subjects.Add(subject);

                    subject = new Subject();
                    subject.Name = "Physics";
                    subject.TotalHours = 60;
                    subject.Grade = grade;
                    subject.SchoolTheme = schoolTheme;

                    context.Subjects.AddOrUpdate(subject);
                    subjects.Add(subject);
                }

                if (schoolTheme.Name == "Humanities (H)")
                {
                    Subject subject = new Subject();
                    subject.Name = "Literature";
                    subject.TotalHours = 100;
                    subject.Grade = grade;
                    subject.SchoolTheme = schoolTheme;

                    context.Subjects.AddOrUpdate(subject);
                    subjects.Add(subject);

                    subject = new Subject();
                    subject.Name = "Languages";
                    subject.TotalHours = 90;
                    subject.Grade = grade;
                    subject.SchoolTheme = schoolTheme;

                    context.Subjects.AddOrUpdate(subject);
                    subjects.Add(subject);

                    subject = new Subject();
                    subject.Name = "Philosophy";
                    subject.TotalHours = 70;
                    subject.Grade = grade;
                    subject.SchoolTheme = schoolTheme;

                    context.Subjects.AddOrUpdate(subject);
                    subjects.Add(subject);
                }
            }

            return subjects;
        }

        private List<SchoolClass> SeedGradeSchoolClasses(
            ApplicationDbContext context, 
            Grade grade,
            IList<SchoolClass> previousYearCurrentGradeClasses,
            int gradeClassesNumber, 
            IList<SchoolTheme> schoolThemes)
        {
            List<SchoolClass> schoolClasses = new List<SchoolClass>();

            if (previousYearCurrentGradeClasses != null && previousYearCurrentGradeClasses.Count() > 0)
            {
                schoolClasses = CopyClassesFromPreviousYearCurrentGrade(context, grade, previousYearCurrentGradeClasses);
            }
            else
            {
                schoolClasses = CreateGradeNewSchoolClasses(context, grade, gradeClassesNumber, schoolThemes);
            }

            return schoolClasses;
        }

        private List<SchoolClass> CopyClassesFromPreviousYearCurrentGrade(
            ApplicationDbContext context, 
            Grade grade, 
            IList<SchoolClass> previousYearCurrentGradeClasses)
        {
            List<SchoolClass> schoolClasses = new List<SchoolClass>();

            foreach (var previousYearCurrentGradeClass in previousYearCurrentGradeClasses)
            {
                SchoolClass schoolClass = new SchoolClass();
                schoolClass.Grade = grade;
                schoolClass.ClassLetter = previousYearCurrentGradeClass.ClassLetter;
                schoolClass.SchoolTheme = previousYearCurrentGradeClass.SchoolTheme;

                context.SchoolClasses.AddOrUpdate(schoolClass);
                schoolClasses.Add(schoolClass);
            }

            return schoolClasses;
        }

        private List<SchoolClass> CreateGradeNewSchoolClasses(
            ApplicationDbContext context, 
            Grade grade, 
            int gradeClassesNumber, 
            IList<SchoolTheme> schoolThemes)
        {
            List<SchoolClass> schoolClasses = new List<SchoolClass>();
            int charANumber = (int)'A';

            for (int currentChar = charANumber; currentChar < charANumber + gradeClassesNumber; currentChar++)
            {
                SchoolClass schoolClass = new SchoolClass();
                schoolClass.Grade = grade;
                schoolClass.ClassLetter = ((char)currentChar).ToString();

                if (schoolClass.Grade.GradeYear < 8)
                {
                    schoolClass.SchoolTheme = schoolThemes.FirstOrDefault(st => st.Name == "General");
                }
                else
                {
                    if (currentChar < charANumber + 2)
                    {
                        schoolClass.SchoolTheme = schoolThemes.FirstOrDefault(st => st.Name == "Science, Technology, Engineering, Math (STEM)");
                    }

                    if (currentChar == charANumber + 2)
                    {
                        schoolClass.SchoolTheme = schoolThemes.FirstOrDefault(st => st.Name == "Medical Careers (MC)");
                    }

                    if (currentChar > charANumber + 2 && currentChar < charANumber + gradeClassesNumber)
                    {
                        schoolClass.SchoolTheme = schoolThemes.FirstOrDefault(st => st.Name == "Humanities (H)");
                    }
                }

                context.SchoolClasses.AddOrUpdate(schoolClass);
                schoolClasses.Add(schoolClass);
            }

            return schoolClasses;
        }

        private List<Student> SeedSchoolClassStudents(
            ApplicationDbContext context,
            SchoolClass oldSchoolClass,
            int classStudentsNumber)
        {
            var students = new List<Student>();

            if (oldSchoolClass != null && oldSchoolClass.Students.Count() > 0)
            {
                students = oldSchoolClass.Students;
            }
            else
            {
                students = CreateClassOfStudents(context, classStudentsNumber);
            }

            return students;
        }

        private List<Student> CreateClassOfStudents(
            ApplicationDbContext context,
            int classStudentsNumber)
        {
            var students = new List<Student>();
            for (int i = 0; i < classStudentsNumber; i++)
            {
                var student = CreateSingleStudent();
                context.Students.Add(student);
                students.Add(student);
            }
            return students;
        }

        private Student CreateSingleStudent()
        {
            var studentProfile = new Student();
            Random rand = new Random();
            studentProfile.Name = this.personNames[rand.Next(0, this.personNames.Count() - 1)];

            // Create Student Role if it does not exist
            if (!roleManager.RoleExists(GlobalConstants.StudentRoleName))
            {
                roleManager.Create(new IdentityRole(GlobalConstants.StudentRoleName));
            }

            // Create Student User with password
            var studentUser = new ApplicationUser();

            studentUser.UserName = "student" + studentCounter.ToString("D4");
            studentUser.Email = "s" + studentCounter.ToString("D4") + "@s.com";
            studentCounter++;

            string password = "111";

            var result = userManager.Create(studentUser, password);

            // Add Student User to Student Role
            if (result.Succeeded)
            {
                userManager.AddToRole(studentUser.Id, GlobalConstants.StudentRoleName);
            }

            // Add Student User to Student Profile
            studentProfile.ApplicationUser = studentUser;

            return studentProfile;

        }

        private List<Teacher> SeedTeachers(ApplicationDbContext context, IList<Grade> currentAcademicYearGrades, IList<Grade> previousAcademicYearGrades)
        {
            var teachers = new List<Teacher>();

            foreach (var grade in currentAcademicYearGrades)
            {
                foreach (var subject in grade.Subjects)
                {
                    Subject previousAcademicYearGradeSubject = new Subject();
                    Teacher teacherProfile = new Teacher();

                    if (previousAcademicYearGrades.Count() > 0)
                    {
                        previousAcademicYearGradeSubject = previousAcademicYearGrades
                            .FirstOrDefault(g => g.GradeYear == grade.GradeYear)
                            .Subjects
                            .FirstOrDefault(s => s.Name == subject.Name);

                        if (previousAcademicYearGradeSubject != null)
                        {
                            subject.Teachers.Add(previousAcademicYearGradeSubject.Teachers.First());
                            context.Subjects.AddOrUpdate(subject);
                        }
                        else
                        {
                            teacherProfile = CreateSingleTeacher(context);
                            subject.Teachers.Add(teacherProfile);
                            context.Teachers.Add(teacherProfile);
                            teachers.Add(teacherProfile);
                        }
                    }
                    else
                    {
                        teacherProfile = CreateSingleTeacher(context);
                        subject.Teachers.Add(teacherProfile);
                        context.Teachers.Add(teacherProfile);
                        teachers.Add(teacherProfile);
                    }
                }
            }

            return teachers;
        }

        private Teacher CreateSingleTeacher(ApplicationDbContext context)
        {
            var teacherProfile = new Teacher();
            Random rand = new Random();
            teacherProfile.Name = this.personNames[rand.Next(0, this.personNames.Count())];

            // Create Teacher Role if it does not exist
            if (!roleManager.RoleExists(GlobalConstants.TeacherRoleName))
            {
                roleManager.Create(new IdentityRole(GlobalConstants.TeacherRoleName));
            }

            // Create Teacher User with password
            var teacherUser = new ApplicationUser();
            string counter = teacherCounter.ToString("D3");
            teacherUser.UserName = "teacher" + counter;
            teacherUser.Email = "t" + counter + "@t.com";
            string password = "111";

            teacherCounter++;

            var result = userManager.Create(teacherUser, password);

            // Add Teacher User to Teacher Role
            if (result.Succeeded)
            {
                userManager.AddToRole(teacherUser.Id, GlobalConstants.TeacherRoleName);
            }

            // Add Teacher User to Teacher Profile
            teacherProfile.ApplicationUser = teacherUser;
            

            return teacherProfile;
        }

        private UserManager<ApplicationUser> CreateUserManager(ApplicationDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // Configure user manager
            // Configure validation logic for usernames
            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = true,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            userManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 3,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            return userManager;
        }
    }
}