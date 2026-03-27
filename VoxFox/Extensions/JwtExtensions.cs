using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace VoxFox.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");

        var secret = jwtSettings["Secret"]
            ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");

        var issuer   = jwtSettings["Issuer"]   ?? throw new InvalidOperationException("JwtSettings:Issuer is not configured.");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JwtSettings:Audience is not configured.");

        var secretKeyBytes       = Encoding.UTF8.GetBytes(secret);
        var symmetricSecurityKey = new SymmetricSecurityKey(secretKeyBytes);

        // Регистрируем ключ как singleton — удобно инжектить в JwtService
        services.AddSingleton(symmetricSecurityKey);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = symmetricSecurityKey,
                    ValidIssuer              = issuer,
                    ValidAudience            = audience,
                    ClockSkew                = TimeSpan.Zero   // без допуска по времени
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogWarning("JWT authentication failed: {Message}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogDebug("JWT challenge: {Error} — {Description}", context.Error, context.ErrorDescription);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogDebug("JWT token validated for {User}", context.Principal?.Identity?.Name);
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
