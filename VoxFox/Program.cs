using Microsoft.AspNetCore.Diagnostics;
using Npgsql;
using OpenTelemetry.Metrics;
using VoxFox.Extensions;

namespace VoxFox;

public sealed class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var apiPort = builder.Configuration["Ports:Api"] ?? "8080";

		// ── Services ─────────────────────────────────────────────────────────
		builder.Services
			.AddCorsPolicy(builder.Configuration)
			.AddApiControllers()
			.AddSwaggerWithAuth()
			.AddJwtAuthentication(builder.Configuration)
			.AddDatabase(builder.Configuration)
			.AddApplicationServices()
			.AddMetrics();

		// ── OpenTelemetry Metrics ───────────────────────────────────────────
		builder.Services.AddOpenTelemetry()
			.WithMetrics(metrics =>
			{
				metrics.AddAspNetCoreInstrumentation();
				metrics.AddNpgsqlInstrumentation();
				metrics.AddPrometheusExporter();
			});

		// ── Build ─────────────────────────────────────────────────────────────
		var app = builder.Build();

		// Обработка исключений
		app.UseExceptionHandler(appError =>
		{
			appError.Run(async context =>
			{
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				context.Response.ContentType = "application/json";

				var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
				if (contextFeature != null)
				{
					var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
					var error = contextFeature.Error;

					logger.LogError(error, "Unhandled exception occurred. TraceId: {TraceId}",
						context.TraceIdentifier);

					var response = new
					{
						error = "An error occurred while processing your request",
						traceId = context.TraceIdentifier,

						message = app.Environment.IsDevelopment() ? error.Message : null,
						stackTrace = app.Environment.IsDevelopment() ? error.StackTrace : null,
						type = app.Environment.IsDevelopment() ? error.GetType().Name : null
					};

					await context.Response.WriteAsJsonAsync(response);
				}
			});
		});

		// ── Middleware pipeline (порядок важен!) ──────────────────────────────
		app.UseCors(CorsExtensions.PolicyName);
		app.UseRouting();

		// OpenTelemetry middleware для метрик
		app.UseOpenTelemetryPrometheusScrapingEndpoint();

		app.UseAuthentication();
		app.UseAuthorization();

		// ── Endpoints ─────────────────────────────────────────────────────────
		app.MapSystemEndpoints();
		app.MapControllers();

		if (!app.Environment.IsProduction())
		{
			app.UseSwaggerWithUi();
			app.MapDebugEndpoints();
		}

		// Настраиваем основной сервер на API порт
		app.Urls.Clear();
		app.Urls.Add($"http://*:{apiPort}");

		var logger = app.Services.GetRequiredService<ILogger<Program>>();
		logger.LogInformation("Starting application - API port: {ApiPort}", apiPort);

		app.Run();
	}
}
