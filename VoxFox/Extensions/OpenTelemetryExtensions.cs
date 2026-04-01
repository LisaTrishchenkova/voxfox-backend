using Npgsql;
using OpenTelemetry.Metrics;

namespace VoxFox.Extensions;

public static class OpenTelemetryExtensions
{
	public static IServiceCollection AddOpenTelemetryMetrics(this IServiceCollection services)
	{
		services.AddOpenTelemetry()
			.WithMetrics(metrics =>
			{
				metrics.AddAspNetCoreInstrumentation();
				metrics.AddNpgsqlInstrumentation();
				metrics.AddPrometheusExporter();
			});

		return services;
	}
}
