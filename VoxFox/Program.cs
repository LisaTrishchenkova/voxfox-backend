using VoxFox.Extensions;
using VoxFox.Services;

namespace VoxFox;

public sealed class Program
{
	public static async Task Main(string[] args)
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
			.AddOpenTelemetryMetrics()
			.AddOpenTelemetryTracing(builder.Configuration)
			.AddElasticsearch(builder.Configuration);

		// ── Build ─────────────────────────────────────────────────────────────
		var app = builder.Build();

		// ── ИНИЦИАЛИЗАЦИЯ Elasticsearch ПРИ ЗАПУСКЕ ───────────────────────────
		using (var scope = app.Services.CreateScope())
		{
			var elasticsearchService = scope.ServiceProvider.GetRequiredService<CourseSearchService>();
			var loggerLocal = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

			try
			{
				loggerLocal.LogInformation("Initializing Elasticsearch index...");

				// ✅ Прямой вызов метода из CourseSearchService
				await elasticsearchService.EnsureIndexExistsAsync();

				// Делаем реиндекс данных из PostgreSQL
				loggerLocal.LogInformation("Starting reindex from PostgreSQL to Elasticsearch...");
				await elasticsearchService.ReindexAllAsync();

				loggerLocal.LogInformation("Elasticsearch initialization completed!");
			}
			catch (System.Exception ex)
			{
				loggerLocal.LogError(ex, "Failed to initialize Elasticsearch");
				// Не останавливаем приложение, просто логируем ошибку
			}
		}

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

		app.MapGet("/debug/otel-status", () => new
		{
			tempoEndpoint = Environment.GetEnvironmentVariable("TEMPO_ENDPOINT"),
			environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
			hasOtel = AppDomain.CurrentDomain.GetAssemblies()
				.Any(a => a.FullName?.Contains("OpenTelemetry") == true)
		});

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

		await app.RunAsync();
	}
}
