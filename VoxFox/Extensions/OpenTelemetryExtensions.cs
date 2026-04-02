using System.Reflection;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace VoxFox.Extensions;

public static class OpenTelemetryExtensions
{
	private const string DefaultServiceName = "voxfox-backend";
	private const string DefaultVersion = "1.0.0";
	private const string DefaultEnvironment = "Development";

	public static IServiceCollection AddOpenTelemetryMetrics(this IServiceCollection services)
	{
		services.AddOpenTelemetry()
			.WithMetrics(metrics =>
			{
				metrics
					.AddAspNetCoreInstrumentation()
					.AddNpgsqlInstrumentation()
					.AddPrometheusExporter();
			});

		return services;
	}

	public static IServiceCollection AddOpenTelemetryTracing(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var serviceName = GetServiceName();
		var serviceNamespace = GetServiceNamespace(configuration);
		var environment = GetEnvironment(configuration);
		var tempoEndpoint = GetTempoEndpoint(configuration);
		var serviceVersion = GetServiceVersion();

		services.AddOpenTelemetry()
			.ConfigureResource(resource => resource
				.AddService(
					serviceName: serviceName,
					serviceVersion: serviceVersion,
					serviceInstanceId: Environment.MachineName)
				.AddAttributes(GetResourceAttributes(configuration, environment, serviceNamespace)))
			.WithTracing(tracing =>
			{
				tracing
					.AddAspNetCoreInstrumentation()
					.AddNpgsql();

				if (string.IsNullOrEmpty(tempoEndpoint))
				{
					tracing.AddConsoleExporter();
				}

				if (!string.IsNullOrEmpty(tempoEndpoint))
				{
					tracing.AddOtlpExporter(options =>
					{
						options.Endpoint = new Uri(tempoEndpoint);
						options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
						options.TimeoutMilliseconds = 5000;
					});
				}
			});

		return services;
	}

	private static Dictionary<string, object> GetResourceAttributes(
		IConfiguration configuration,
		string environment,
		string serviceNamespace)
	{
		var attributes = new Dictionary<string, object>
		{
			["deployment.environment"] = environment,
			["host.name"] = Environment.MachineName,
			["telemetry.sdk.name"] = "opentelemetry",
			["telemetry.sdk.language"] = "dotnet",
			["telemetry.sdk.version"] = GetTelemetrySdkVersion()
		};

		if (!string.IsNullOrEmpty(serviceNamespace))
		{
			attributes["service.namespace"] = serviceNamespace;
		}

		var customAttributes = configuration.GetSection("OpenTelemetry:ResourceAttributes");
		foreach (var child in customAttributes.GetChildren())
		{
			var value = child.Value;
			if (!string.IsNullOrEmpty(value))
			{
				attributes[child.Key] = value;
			}
		}

		return attributes;
	}

	private static string GetServiceName()
	{
		var otelServiceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME");
		if (!string.IsNullOrEmpty(otelServiceName))
			return otelServiceName;

		return Assembly.GetEntryAssembly()?.GetName().Name ?? DefaultServiceName;
	}

	private static string GetServiceVersion()
	{
		return Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? DefaultVersion;
	}

	private static string GetServiceNamespace(IConfiguration configuration)
	{
		var otelNamespace = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAMESPACE");
		if (!string.IsNullOrEmpty(otelNamespace))
			return otelNamespace;

		return configuration["OpenTelemetry:ServiceNamespace"] ?? "";
	}

	private static string GetEnvironment(IConfiguration configuration)
	{
		return configuration["ASPNETCORE_ENVIRONMENT"] ?? DefaultEnvironment;
	}

	private static string GetTempoEndpoint(IConfiguration configuration)
	{
		var endpoint = Environment.GetEnvironmentVariable("TEMPO_ENDPOINT");
		if (!string.IsNullOrEmpty(endpoint))
			return endpoint;

		return configuration["OpenTelemetry:TempoEndpoint"] ?? "";
	}

	private static string GetTelemetrySdkVersion()
	{
		var assembly = typeof(TracerProviderBuilder).Assembly;
		return assembly.GetName().Version?.ToString() ?? "1.0.0";
	}
}
