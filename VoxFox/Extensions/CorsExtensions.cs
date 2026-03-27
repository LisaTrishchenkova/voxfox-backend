namespace VoxFox.Extensions;

public static class CorsExtensions
{
	public const string PolicyName = "AllowFrontend";

	public static IServiceCollection AddCorsPolicy(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// Читаем origins из конфига — не хардкодим в коде
		var allowedOrigins = configuration
			                     .GetSection("Cors:AllowedOrigins")
			                     .Get<string[]>()
		                     ?? [];

		services.AddCors(options =>
		{
			options.AddPolicy(PolicyName, policy =>
			{
				policy
					.WithOrigins(allowedOrigins)
					.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowCredentials();
			});
		});

		return services;
	}
}
