using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Models.Entities
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<TaskSubmission> TaskSubmissions { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<CourseReviewHistory> CourseReviewHistories { get; set; }
        public DbSet<CourseDraft> CourseDrafts { get; set; }
        public DbSet<DraftSection> DraftSections { get; set; }
        public DbSet<DraftLesson> DraftLessons { get; set; }
        public DbSet<DraftTask> DraftTasks { get; set; }
        public DbSet<DraftTag> DraftTags { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Section>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tag>()
                .HasOne(t => t.Course)
                .WithMany(c => c.Tags)
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lesson>()
               .HasOne(l => l.Section)
               .WithMany(s => s.Lessons)
               .HasForeignKey(l => l.SectionId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Category)
                .WithMany(c => c.Courses)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Courses)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150);
                entity.HasIndex(e => e.Email)
                    .IsUnique();
                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Role)
                    .HasConversion<string>()
                    .HasDefaultValue(UserRole.Student);
                entity.Property(e => e.Bio)
                    .IsRequired(false)
                    .HasMaxLength(500);
                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.Property(e => e.Balance)
                    .HasColumnType("numeric(10,2)")
                    .HasDefaultValue(0);
            });
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.Description)
                    .HasMaxLength(500);
                entity.Property(e => e.FullDescription)
                    .IsRequired(false);
                entity.Property(e => e.CoverImageUrl)
                    .IsRequired(false)
                    .HasMaxLength(500);
                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("numeric(10,2)")
                    .HasDefaultValue(0);
                entity.Property(e => e.Level)
                    .HasConversion<string>()
                    .HasDefaultValue(CourseLevel.Beginner);
                entity.Property(e => e.CertificateEnabled)
                    .HasDefaultValue(false);
                entity.Property(e => e.EnrollmentCount)
                    .HasDefaultValue(0);
                entity.Property(e => e.Rating)
                    .HasColumnType("numeric(3,2)")
                    .HasDefaultValue(0);
                entity.Property(e => e.DurationMinutes)
                    .HasDefaultValue(0);
                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.IsDeleted);
                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasDefaultValue(CourseStatus.Draft);
                entity.Property(e => e.CategoryId);
                entity.Property(e => e.AuthorId)
                    .IsRequired(false);
                entity.Property(e => e.PublishedAt)
                    .IsRequired(false)
                    .HasColumnType("timestamp with time zone");
                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasQueryFilter(c => !c.IsDeleted);
                entity.Property(e => e.ReviewerId).IsRequired(false);
                entity.Property(e => e.ReviewStartedAt)
                    .IsRequired(false)
                    .HasColumnType("timestamp with time zone");
                entity.Property(e => e.ReviewCount).HasDefaultValue(0);

                modelBuilder.Entity<Course>()
                    .HasOne(c => c.Reviewer)
                    .WithMany()
                    .HasForeignKey(c => c.ReviewerId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            }
            );

            modelBuilder.Entity<Section>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.Description)
                    .IsRequired();
                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CourseId)
                    .HasColumnType("uuid");
                entity.Property(e => e.IsDeleted);
                entity.HasQueryFilter(e => !e.IsDeleted);
            }
            );

            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.Description)
                    .IsRequired();
                entity.Property(e => e.Content)
                    .IsRequired();
                entity.Property(e => e.IsDeleted);
                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.Property(e => e.OrderIndex)
                    .HasDefaultValue(0);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");

            });

            //  modelBuilder.Entity<Author>(entity =>
            // {
            //     entity.HasKey(e => e.Id);
            //
            //     entity.Property(e => e.Name)
            //              .IsRequired()
            //             .HasMaxLength(200);
            //
            //     entity.Property(e => e.Id)
            //             .HasColumnType("uuid")
            //             .HasDefaultValueSql("gen_random_uuid()");
            // });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasDefaultValue(EnrollmentStatus.Active);

                entity.Property(e => e.EnrolledAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.CompletedAt)
                    .IsRequired(false)
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.ProgressPercent)
                    .HasDefaultValue(0);

                // уникальный индекс — один пользователь не может записаться дважды
                entity.HasIndex(e => new { e.UserId, e.CourseId })
                    .IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // один пользователь не может добавить курс в избранное дважды
                entity.HasIndex(e => new { e.UserId, e.CourseId })
                    .IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<TaskEntity>(entity =>
 {
     entity.HasKey(e => e.Id);

     entity.Property(e => e.Id)
         .HasColumnType("uuid")
         .HasDefaultValueSql("gen_random_uuid()");

     entity.Property(e => e.Type)
         .HasConversion<string>()
         .IsRequired();

     entity.Property(e => e.Question)
         .IsRequired()
         .HasMaxLength(1000);

     entity.Property(e => e.Options)
         .HasColumnType("jsonb")
         .IsRequired(false);

     entity.Property(e => e.CorrectIndex)
         .IsRequired(false);

     entity.Property(e => e.CorrectIndexes)
         .HasColumnType("jsonb")
         .IsRequired(false);

     entity.Property(e => e.CorrectAnswer)
         .IsRequired(false)
         .HasMaxLength(1000);

     entity.Property(e => e.Explanation)
         .IsRequired(false)
         .HasMaxLength(2000);

     entity.Property(e => e.Hints)
         .HasColumnType("jsonb")
         .IsRequired(false);

     entity.Property(e => e.Points)
         .HasDefaultValue(1);

     entity.Property(e => e.OrderIndex)
         .IsRequired();

     entity.Property(e => e.IsRequired)
         .HasDefaultValue(true);

     entity.Property(e => e.CreatedAt)
         .IsRequired()
         .HasColumnType("timestamp with time zone")
         .HasDefaultValueSql("CURRENT_TIMESTAMP");

     entity.HasOne(e => e.Lesson)
         .WithMany()
         .HasForeignKey(e => e.LessonId)
         .OnDelete(DeleteBehavior.Cascade);
     entity.Property(e => e.IsDeleted)
         .HasDefaultValue(false);

     entity.HasQueryFilter(e => !e.IsDeleted);

     entity.HasOne(e => e.Lesson)
         .WithMany()
         .HasForeignKey(e => e.LessonId)
         .OnDelete(DeleteBehavior.Cascade);
 });

            modelBuilder.Entity<TaskSubmission>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.AnswerIndex)
                    .IsRequired(false);

                entity.Property(e => e.AnswerIndexes)
                    .HasColumnType("jsonb")
                    .IsRequired(false);

                entity.Property(e => e.AnswerText)
                    .IsRequired(false)
                    .HasMaxLength(1000);

                entity.Property(e => e.IsCorrect)
                    .IsRequired();

                entity.Property(e => e.Score)
                    .HasDefaultValue(0);

                entity.Property(e => e.AttemptNumber)
                    .HasDefaultValue(1);

                entity.Property(e => e.SubmittedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Task)
                    .WithMany(t => t.Submissions)
                    .HasForeignKey(e => e.TaskId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<LessonProgress>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.CompletedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // уникальный индекс — студент не может пройти урок дважды
                entity.HasIndex(e => new { e.UserId, e.LessonId })
                    .IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Lesson)
                    .WithMany()
                    .HasForeignKey(e => e.LessonId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Enrollment)
                    .WithMany()
                    .HasForeignKey(e => e.EnrollmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.Rating)
                    .IsRequired();

                entity.Property(e => e.Comment)
                    .IsRequired(false)
                    .HasMaxLength(2000);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired(false)
                    .HasColumnType("timestamp with time zone");

                // один студент — один отзыв на курс
                entity.HasIndex(e => new { e.UserId, e.CourseId })
                    .IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(e => e.AnswerText)
                    .IsRequired(false)
                    .HasMaxLength(4000);

                entity.Property(e => e.AnsweredAt)
                    .IsRequired(false)
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);

                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Lesson)
                    .WithMany()
                    .HasForeignKey(e => e.LessonId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AnsweredBy)
                    .WithMany()
                    .HasForeignKey(e => e.AnsweredById)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.Type)
                    .HasConversion<string>();

                entity.Property(e => e.IsRead)
                    .HasDefaultValue(false);

                entity.Property(e => e.RelatedEntityId)
                    .IsRequired(false);

                entity.Property(e => e.RelatedCourseId)
                    .IsRequired(false);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Certificate>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.VerificationToken)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(e => e.VerificationToken)
                    .IsUnique();

                // один сертификат на один enrollment
                entity.HasIndex(e => e.EnrollmentId)
                    .IsUnique();

                entity.Property(e => e.IssuedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Enrollment)
                    .WithMany()
                    .HasForeignKey(e => e.EnrollmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<CourseDraft>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");
    entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
    entity.Property(e => e.FullDescription).IsRequired(false);
    entity.Property(e => e.CoverImageUrl).IsRequired(false).HasMaxLength(500);
    entity.Property(e => e.Price).HasColumnType("numeric(10,2)").HasDefaultValue(0);
    entity.Property(e => e.Level).HasConversion<string>().HasDefaultValue(CourseLevel.Beginner);
    entity.Property(e => e.CertificateEnabled).HasDefaultValue(false);
    entity.Property(e => e.Status).HasConversion<string>().HasDefaultValue(DraftStatus.Draft);
    entity.Property(e => e.CreatedAt).IsRequired().HasColumnType("timestamp with time zone");
    entity.Property(e => e.UpdatedAt).IsRequired().HasColumnType("timestamp with time zone");
    entity.HasOne(e => e.Course).WithMany().HasForeignKey(e => e.CourseId).OnDelete(DeleteBehavior.Cascade);
    entity.HasOne(e => e.Author).WithMany().HasForeignKey(e => e.AuthorId).OnDelete(DeleteBehavior.Restrict);
});

modelBuilder.Entity<DraftSection>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");
    entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Description).IsRequired();
    entity.Property(e => e.OriginalSectionId).IsRequired(false);
    entity.Property(e => e.OrderIndex).HasDefaultValue(0);
    entity.HasOne(e => e.Draft).WithMany(d => d.Sections).HasForeignKey(e => e.DraftId).OnDelete(DeleteBehavior.Cascade);
});

modelBuilder.Entity<DraftLesson>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");
    entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Description).IsRequired();
    entity.Property(e => e.Content).IsRequired(false);
    entity.Property(e => e.OriginalLessonId).IsRequired(false);
    entity.Property(e => e.OrderIndex).HasDefaultValue(0);
    entity.HasOne(e => e.DraftSection).WithMany(s => s.Lessons).HasForeignKey(e => e.DraftSectionId).OnDelete(DeleteBehavior.Cascade);
});

modelBuilder.Entity<DraftTask>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");
    entity.Property(e => e.Type).HasConversion<string>().IsRequired();
    entity.Property(e => e.Question).IsRequired().HasMaxLength(1000);
    entity.Property(e => e.Options).HasColumnType("jsonb").IsRequired(false);
    entity.Property(e => e.CorrectIndex).IsRequired(false);
    entity.Property(e => e.CorrectIndexes).HasColumnType("jsonb").IsRequired(false);
    entity.Property(e => e.CorrectAnswer).IsRequired(false).HasMaxLength(1000);
    entity.Property(e => e.Explanation).IsRequired(false).HasMaxLength(2000);
    entity.Property(e => e.Points).HasDefaultValue(1);
    entity.Property(e => e.IsRequired).HasDefaultValue(true);
    entity.Property(e => e.OrderIndex).HasDefaultValue(0);
    entity.Property(e => e.OriginalTaskId).IsRequired(false);
    entity.HasOne(e => e.DraftLesson).WithMany(l => l.Tasks).HasForeignKey(e => e.DraftLessonId).OnDelete(DeleteBehavior.Cascade);
});

modelBuilder.Entity<DraftTag>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");
    entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
    entity.HasOne(e => e.Draft).WithMany(d => d.Tags).HasForeignKey(e => e.DraftId).OnDelete(DeleteBehavior.Cascade);
});
modelBuilder.Entity<Transaction>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Id)
        .HasColumnType("uuid")
        .HasDefaultValueSql("gen_random_uuid()");
    entity.Property(e => e.Amount)
        .HasColumnType("numeric(10,2)")
        .IsRequired();
    entity.Property(e => e.TotalAmount)
        .HasColumnType("numeric(10,2)")
        .IsRequired(false);
    entity.Property(e => e.TeacherAmount)
        .HasColumnType("numeric(10,2)")
        .IsRequired(false);
    entity.Property(e => e.PlatformAmount)
        .HasColumnType("numeric(10,2)")
        .IsRequired(false);
    entity.Property(e => e.Type)
        .HasConversion<string>();
    entity.Property(e => e.IsRefunded)
        .HasDefaultValue(false);
    entity.Property(e => e.OriginalTransactionId)
        .IsRequired(false);
    entity.Property(e => e.CreatedAt)
        .IsRequired()
        .HasColumnType("timestamp with time zone")
        .HasDefaultValueSql("CURRENT_TIMESTAMP");

    entity.HasOne(e => e.User)
        .WithMany()
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(e => e.Course)
        .WithMany()
        .HasForeignKey(e => e.CourseId)
        .IsRequired(false)
        .OnDelete(DeleteBehavior.SetNull);

    entity.HasOne(e => e.OriginalTransaction)
        .WithMany()
        .HasForeignKey(e => e.OriginalTransactionId)
        .IsRequired(false)
        .OnDelete(DeleteBehavior.Restrict);
});

            base.OnModelCreating(modelBuilder);
        }

    }
}
