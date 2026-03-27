using Prometheus;
using VoxFox.Extensions;

namespace VoxFox;

public sealed class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// ── Services ─────────────────────────────────────────────────────────
		builder.Services
			.AddCorsPolicy(builder.Configuration)
			.AddApiControllers()
			.AddSwaggerWithAuth()
			.AddJwtAuthentication(builder.Configuration)
			.AddDatabase(builder.Configuration)
			.AddApplicationServices()
			.AddMetrics();

		// ── Build ─────────────────────────────────────────────────────────────
		var app = builder.Build();

		// ── Middleware pipeline (порядок важен!) ──────────────────────────────
		app.UseCors(CorsExtensions.PolicyName);
		app.UseRouting();
		app.UseHttpMetrics(options => options.ReduceStatusCodeCardinality());

		app.UseAuthentication();
		app.UseAuthorization();

		// ── Endpoints ─────────────────────────────────────────────────────────
		app.MapSystemEndpoints();
		app.MapControllers();
		app.MapMetrics().RequireHost("*:9090");

		// Swagger + Debug только в не-prod окружениях
		if (!app.Environment.IsProduction())
		{
			app.UseSwaggerWithUi();
			app.MapDebugEndpoints();
		}

		// ── Ports ─────────────────────────────────────────────────────────────
		app.Urls.Add("http://+:8080"); // основной трафик
		app.Urls.Add("http://+:9090"); // метрики Prometheus

		app.Run();
	}
}
