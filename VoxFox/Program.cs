using System.Reflection;
using System.Text.Json.Serialization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VoxFox.Services;
using VoxFox.Models.Entities;
using Microsoft.EntityFrameworkCore;
using VoxFox.Repositories;
using VoxFox.Interfaces.Section;
using Microsoft.OpenApi;

namespace VoxFox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
          {
              options.AddPolicy("AllowFrontend",
                  policy =>
                  {
                      policy.WithOrigins(
                              "http://localhost:5001",
                              "https://voxfox.dev.bafid.app",
                              "https://voxfox.staging.bafid.app",
                              "https://voxfox.bafid.app")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                  });
          });

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // SettingJWT(builder);

            // builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "VoxFox API",
                    Version = "v1"
                });

                c.UseInlineDefinitionsForEnums();

                // c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                // {
                //     Description = @"JWT Authorization header using the Bearer scheme.
                //                   Enter your token in the text input below.
                //                   Example: 'Bearer 12345abcdef'",
                //     Name = "Authorization",
                //     In = ParameterLocation.Header,
                //     Type = SecuritySchemeType.Http,
                //     Scheme = "Bearer"
                // });

                // c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                // {
                //     {
                //         new OpenApiSecurityScheme
                //         {
                //             Reference = new OpenApiReference
                //             {
                //                 Type = ReferenceType.SecurityScheme,
                //                 Id = "Bearer"
                //             },
                //             Scheme = "oauth2",
                //             Name = "Bearer",
                //             In = ParameterLocation.Header,
                //         },

                //         new List<string>()
                //     }
                // });

                // var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            // builder.Services.AddScoped<IJwtService, JwtService>();

            ConfigureDatabase(builder);

            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<ISectionRepository, SectionRepository>();
            builder.Services.AddScoped<ISectionService, SectionService>();
            builder.Services.AddScoped<ILessonRepository, LessonRepository>();
            builder.Services.AddScoped<ILessonService, LessonService>();
            var app = builder.Build();

            app.UseCors("AllowFrontend");
            app.UseRouting();

            app.MapGet("/version", () => new
            {
                Version = Environment.GetEnvironmentVariable("APP_VERSION"),
                Environment = app.Environment.EnvironmentName
            });

            app.MapGet("/healthz", () => Results.Ok(new { status = "alive" }));

            app.MapGet("/health", () => new
            {
                Status = "healthy",
                Version = Environment.GetEnvironmentVariable("APP_VERSION"),
                // Database = CheckDatabase() ? "connected" : "disconnected",
                Timestamp = DateTime.UtcNow
            });

            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Staging"))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "VoxFox API V1");
                    c.RoutePrefix = "swagger";
                });

                app.UseReDoc(o =>
                {
                    o.DocumentTitle = "111";
                    o.SpecUrl = "/swagger/v1/swagger.json";
                    o.RoutePrefix = "api-docs";

                });

                app.MapGet("/debug", (HttpContext httpContext) =>
                {
                    // Получаем все зарегистрированные CORS политики
                    var corsOptions = app.Services.GetRequiredService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsPolicyProvider>();

                    // Информация о текущем запросе
                    var currentOrigin = httpContext.Request.Headers["Origin"].FirstOrDefault() ?? "нет origin";
                    var currentMethod = httpContext.Request.Method;

                    // Информация о CORS заголовках в ответе
                    var corsHeaders = httpContext.Response.Headers
                        .Where(h => h.Key.StartsWith("Access-Control-", StringComparison.OrdinalIgnoreCase))
                        .ToDictionary(h => h.Key, h => h.Value.ToString());

                    // Собираем информацию о всех CORS политиках
                    var corsPolicies = new List<object>();

                    // Используем рефлексию для получения зарегистрированных политик
                    var serviceProvider = app.Services;
                    var corsService = serviceProvider.GetService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsService>();

                    // Пытаемся получить политику "AllowFrontend"
                    var policy = app.Services.GetRequiredService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsPolicyProvider>()
                        .GetPolicyAsync(httpContext, "AllowFrontend")
                        .GetAwaiter()
                        .GetResult();

                    if (policy != null)
                    {
                        corsPolicies.Add(new
                        {
                            Name = "AllowFrontend",
                            Origins = policy.Origins?.ToList() ?? new List<string>(),
                            Methods = policy.Methods?.ToList() ?? new List<string>(),
                            Headers = policy.Headers?.ToList() ?? new List<string>(),
                            ExposedHeaders = policy.ExposedHeaders?.ToList() ?? new List<string>(),
                            SupportsCredentials = policy.SupportsCredentials,
                            PreflightMaxAge = policy.PreflightMaxAge?.TotalSeconds ?? 0,
                            IsDefaultPolicy = false
                        });
                    }

                    return new
                    {
                        // Информация о сборке
                        Commit = Environment.GetEnvironmentVariable("GIT_COMMIT") ?? "не задан",
                        BuildDate = Environment.GetEnvironmentVariable("BUILD_DATE") ?? "не задана",

                        // Информация о среде
                        Environment = app.Environment.EnvironmentName,
                        ApplicationName = app.Environment.ApplicationName,
                        ContentRootPath = app.Environment.ContentRootPath,

                        // Информация о CORS
                        Cors = new
                        {
                            CurrentRequest = new
                            {
                                Origin = currentOrigin,
                                Method = currentMethod,
                                IsPreflightRequest = httpContext.Request.Method == "OPTIONS",
                                HasOriginHeader = httpContext.Request.Headers.ContainsKey("Origin")
                            },
                            ResponseHeaders = corsHeaders,
                            ConfiguredPolicies = corsPolicies,
                            IsPolicyApplied = corsHeaders.Any(),

                            // Дополнительная отладочная информация
                            Debug = new
                            {
                                // Проверяем, разрешен ли текущий origin
                                CurrentOriginAllowed = policy?.Origins?.Contains(currentOrigin) ?? false,
                                PolicyExists = policy != null,
                                MiddlewareOrder = "app.UseCors() вызван до app.UseRouting()"
                            }
                        },

                        // Все переменные окружения (VITE_ и другие)
                        AllEnvVars = Environment.GetEnvironmentVariables()
                            .Cast<System.Collections.DictionaryEntry>()
                            .ToDictionary(
                                kv => kv.Key.ToString() ?? "",
                                kv => kv.Value?.ToString() ?? ""
                            ),

                        // Информация о всех зарегистрированных endpoint'ах
                        Endpoints = app.Services.GetService<Microsoft.AspNetCore.Routing.EndpointDataSource>()?.Endpoints
                            .Select(e => e.DisplayName)
                            .Where(name => !string.IsNullOrEmpty(name))
                            .ToList() ?? new List<string?>(),

                        // Заголовки запроса
                        RequestHeaders = httpContext.Request.Headers
                            .ToDictionary(h => h.Key, h => h.Value.ToString()),

                        Timestamp = DateTime.UtcNow
                    };
                })
                .WithName("Debug")
                .WithDisplayName("Debug endpoint with CORS info");
            }
            //app.UseHttpsRedirection();

            // app.UseAuthentication();
            // app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void SettingJWT(WebApplicationBuilder builder)
        {
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            // var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);
            var secretKey = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

            var symmetricSecurityKey = new SymmetricSecurityKey(secretKeyBytes);
            builder.Services.AddSingleton(symmetricSecurityKey);


            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    ClockSkew = TimeSpan.Zero
                };

                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        Console.WriteLine($"Exception: {context.Exception}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"Challenge: {context.Error}, {context.ErrorDescription}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully");
                        return Task.CompletedTask;
                    }
                };
            });
        }

        private static void ConfigureDatabase(WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Database connection string is not configured.");

            builder.Services.AddDbContext<ApplicationContext>(options =>
                options
                    .UseNpgsql(connectionString, o =>
                        o.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName)));

            builder.Services.AddHostedService<MigrationHostedService>();
            // builder.Services.AddHostedService<DatabaseInitializerService>();
        }
    }
}
