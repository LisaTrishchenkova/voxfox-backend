using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using VoxFox.Features;
using VoxFox.Interfaces;
using VoxFox.Interfaces.Achievement;
using VoxFox.Interfaces.Admin;
using VoxFox.Interfaces.Balance;
using VoxFox.Interfaces.Certificate;
using VoxFox.Interfaces.DraftCourse;
using VoxFox.Interfaces.Enrollment;
using VoxFox.Interfaces.Lesson;
using VoxFox.Interfaces.Moderation;
using VoxFox.Interfaces.Notification;
using VoxFox.Interfaces.Question;
using VoxFox.Interfaces.Review;
using VoxFox.Interfaces.Section;
using VoxFox.Interfaces.Task;
using VoxFox.Interfaces.Teacher;
using VoxFox.Models.Entities;
using VoxFox.Repositories;
using VoxFox.Services;
using VoxFox.Services.Course;

namespace VoxFox.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services)
	{
		services.AddScoped<ICourseRepository, CourseRepository>();
		services.AddScoped<ICourseService, CourseService>();
		services.AddScoped<ISectionRepository, SectionRepository>();
		services.AddScoped<ISectionService, SectionService>();
		services.AddScoped<ILessonRepository, LessonRepository>();
		services.AddScoped<ILessonService, LessonService>();
		services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
		services.AddScoped<IEnrollmentService, EnrollmentService>();
		services.AddScoped<ITaskRepository, TaskRepository>();
		services.AddScoped<IFavoriteRepository, FavoriteRepository>();
		services.AddScoped<IFavoriteService, FavoriteService>();
		services.AddMediatR(cfg =>
			cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
		services.AddScoped<IJwtService, JwtService>();
		services.AddScoped<CourseSearchService>();
		services.AddScoped<ILessonProgressRepository, LessonProgressRepository>();
		services.AddScoped<IReviewRepository, ReviewRepository>();
		services.AddScoped<IReviewService, ReviewService>();
		services.AddScoped<IQuestionRepository, QuestionRepository>();
		services.AddScoped<IQuestionService, QuestionService>();
		services.AddScoped<INotificationRepository, NotificationRepository>();
		services.AddScoped<INotificationService, NotificationService>();
		services.AddScoped<ICertificateRepository, CertificateRepository>();
		services.AddScoped<ICertificateService, CertificateService>();
		services.AddScoped<IAdminService, AdminService>();
		services.AddScoped<IModerationService, ModerationService>();
		services.AddHostedService<ModerationCleanupJob>();
		services.AddScoped<ICourseDraftRepository, CourseDraftRepository>();
		services.AddScoped<ICourseDraftService, CourseDraftService>();
		services.AddScoped<IBalanceRepository, BalanceRepository>();
		services.AddScoped<IBalanceService, BalanceService>();
		services.AddScoped<ITeacherRepository, TeacherRepository>();
		services.AddScoped<ITeacherService, TeacherService>();
		services.AddScoped<IAchievementRepository, AchievementRepository>();
		services.AddScoped<IAchievementService, AchievementService>();
		return services;
	}

	public static IServiceCollection AddDatabase(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection")
							   ?? throw new InvalidOperationException(
								   "Connection string 'DefaultConnection' is not configured.");

		services.AddDbContext<ApplicationContext>(options =>
			options.UseNpgsql(
				connectionString,
				o => o.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName)));

		services.AddHostedService<MigrationHostedService>();

		return services;
	}

	public static IServiceCollection AddApiControllers(this IServiceCollection services)
	{
		services
			.AddControllers()
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
			});

		services.AddEndpointsApiExplorer();

		return services;
	}
}
