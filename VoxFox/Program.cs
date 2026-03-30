using Microsoft.AspNetCore.Diagnostics;
using Prometheus;
using VoxFox.Extensions;

namespace VoxFox;

public sealed class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var metricsPort = builder.Configuration["Ports:Metrics"] ?? "9090";

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

		// TODO: вынести как ниже все
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
		app.UseHttpMetrics(options => options.ReduceStatusCodeCardinality());

		app.UseAuthentication();
		app.UseAuthorization();

		// ── Endpoints ─────────────────────────────────────────────────────────
		app.MapSystemEndpoints();
		app.MapControllers();

		app.MapMetrics().RequireHost($"*:{metricsPort}");

		app.Urls.Add($"http://+:{metricsPort}");

		if (!app.Environment.IsProduction())
		{
			app.UseSwaggerWithUi();
			app.MapDebugEndpoints();
		}

		app.Run();
	}
}
