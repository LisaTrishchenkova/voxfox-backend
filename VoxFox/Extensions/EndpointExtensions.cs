using Microsoft.AspNetCore.Cors.Infrastructure;

namespace VoxFox.Extensions;

/// <summary>
/// Регистрация всех minimal API endpoints.
/// </summary>
public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/version", (IWebHostEnvironment env) => new
        {
            Version     = Environment.GetEnvironmentVariable("APP_VERSION"),
            Environment = env.EnvironmentName
        })
        .DisableHttpMetrics()
        .WithTags("System");

        app.MapGet("/healthz", () => Results.Ok(new { status = "alive" }))
            .DisableHttpMetrics()
            .WithTags("System");

        app.MapGet("/health", (IWebHostEnvironment env) => new
        {
            Status    = "healthy",
            Version   = Environment.GetEnvironmentVariable("APP_VERSION"),
            Timestamp = DateTime.UtcNow,
            Environment = env.EnvironmentName
        })
        .DisableHttpMetrics()
        .WithTags("System");

        return app;
    }

    /// <summary>
    /// Debug endpoint — только для Development и Staging.
    /// </summary>
    public static IEndpointRouteBuilder MapDebugEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/debug", async (HttpContext httpContext, IWebHostEnvironment env) =>
        {
            var policyProvider = httpContext.RequestServices
                .GetRequiredService<ICorsPolicyProvider>();

            var currentOrigin = httpContext.Request.Headers["Origin"].FirstOrDefault() ?? "no origin";

            var corsHeaders = httpContext.Response.Headers
                .Where(h => h.Key.StartsWith("Access-Control-", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(h => h.Key, h => h.Value.ToString());

            var policy = await policyProvider.GetPolicyAsync(httpContext, CorsExtensions.PolicyName);

            return new
            {
                Commit    = Environment.GetEnvironmentVariable("GIT_COMMIT")  ?? "not set",
                BuildDate = Environment.GetEnvironmentVariable("BUILD_DATE")  ?? "not set",
                Environment = env.EnvironmentName,
                ApplicationName = env.ApplicationName,

                Cors = new
                {
                    CurrentRequest = new
                    {
                        Origin              = currentOrigin,
                        Method              = httpContext.Request.Method,
                        IsPreflightRequest  = httpContext.Request.Method == "OPTIONS",
                        HasOriginHeader     = httpContext.Request.Headers.ContainsKey("Origin")
                    },
                    ResponseHeaders      = corsHeaders,
                    CurrentOriginAllowed = policy?.Origins?.Contains(currentOrigin) ?? false,
                    PolicyExists         = policy != null,
                    ConfiguredOrigins    = policy?.Origins?.ToList() ?? []
                },

                Endpoints = app.ServiceProvider
                    .GetService<EndpointDataSource>()?.Endpoints
                    .Select(e => e.DisplayName)
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList() ?? [],

                RequestHeaders = httpContext.Request.Headers
                    .ToDictionary(h => h.Key, h => h.Value.ToString()),

                Timestamp = DateTime.UtcNow
            };
        })
        .DisableHttpMetrics()
        .WithName("Debug")
        .WithTags("Debug");

        return app;
    }
}
