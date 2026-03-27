using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using VoxFox.Interfaces;
using VoxFox.Interfaces.Course;
using VoxFox.Interfaces.Lesson;
using VoxFox.Interfaces.Section;
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
		services.AddScoped<IJwtService, JwtService>();

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
