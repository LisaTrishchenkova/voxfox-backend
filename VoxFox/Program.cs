

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VoxFox.Interfaces;
using VoxFox.Services;
using VoxFox.Models.Entities;
using Microsoft.EntityFrameworkCore;

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
                      policy.WithOrigins("https://voxfox.bafid.app", "http://localhost:5001")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                  });
          });



            // Add services to the container.

            builder.Services.AddControllers();

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

            // ConfigureDatabase(builder);

            // builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            // builder.Services.AddScoped<ICourseService, CourseService>();
            var app = builder.Build();

            app.UseRouting();
            app.UseCors("AllowFrontend");

            // Configure the HTTP request pipeline.
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

        // private static void ConfigureDatabase(WebApplicationBuilder builder)
        // {
        //     var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Database connection string is not configured.");

        //     builder.Services.AddDbContext<ApplicationContext>(options =>
        //         options
        //             .UseNpgsql(connectionString, o =>
        //                 o.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName)));

        //     builder.Services.AddHostedService<MigrationHostedService>();
        //     // builder.Services.AddHostedService<DatabaseInitializerService>();
        // }
    }
}
