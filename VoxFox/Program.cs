using VoxFox.Extensions;

namespace VoxFox;

public sealed class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var apiPort = builder.Configuration["Ports:Api"] ?? "8080";
		var metricsPort = int.Parse(builder.Configuration["Ports:Metrics"] ?? "9090");

		// ── Services ──────────────────────────────────────────────────────────
		builder.Services
			.AddCorsPolicy(builder.Configuration)
			.AddApiControllers()
			.AddApiDocumentation()
			.AddJwtAuthentication(builder.Configuration)
			.AddDatabase(builder.Configuration)
			.AddApplicationServices()
			.AddMetrics()
			.AddOpenTelemetryMetrics();

		// ── Build ─────────────────────────────────────────────────────────────
		var app = builder.Build();

		// ── Middleware pipeline (порядок важен!) ──────────────────────────────
		app.UseGlobalExceptionHandler(app.Environment);

		app.UseCors(CorsExtensions.PolicyName);
		app.UseRouting();

		app.UseOpenTelemetryPrometheusScrapingEndpoint(context => context.Connection.LocalPort == metricsPort);

		app.UseAuthentication();
		app.UseAuthorization();

		// ── Endpoints ─────────────────────────────────────────────────────────
		app.MapSystemEndpoints();
		app.MapControllers();

		if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
		{
			app.UseApiDocumentation();
			app.MapDebugEndpoints();
		}

		app.Urls.Clear();
		app.Urls.Add($"http://*:{apiPort}");
		app.Urls.Add($"http://*:{metricsPort}");

		var logger = app.Services.GetRequiredService<ILogger<Program>>();
		logger.LogInformation("Starting application - API port: {ApiPort}", apiPort);

		app.Run();
	}
}
