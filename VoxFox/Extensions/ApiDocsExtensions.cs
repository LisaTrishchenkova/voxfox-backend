using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace VoxFox.Extensions;

public static class ApiDocsExtensions
{
	public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
	{
		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "VoxFox API",
				Version = "v1"
			});

			c.UseInlineDefinitionsForEnums();
			// Bearer auth в Swagger UI
			c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Type = SecuritySchemeType.Http,
				Scheme = "bearer",
				BearerFormat = "JWT",
				Description = "Введите JWT токен (без префикса Bearer)"
			});

			c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
			{
				{ new OpenApiSecuritySchemeReference("Bearer", document), [] }
			});
		});

		return services;
	}

	public static WebApplication UseApiDocumentation(this WebApplication app)
	{
		app.UseSwagger();

		app.UseSwaggerUI(c =>
		{
			c.SwaggerEndpoint("/swagger/v1/swagger.json", "VoxFox API V1");
			c.RoutePrefix = "swagger";
		});

		app.UseReDoc(o =>
		{
			o.DocumentTitle = "VoxFox API — ReDoc";
			o.SpecUrl = "/swagger/v1/swagger.json";
			o.RoutePrefix = "api-docs";
		});

		app.MapSwagger("/openapi/{documentName}.json");
		app.MapScalarApiReference(o =>
		{
			o.WithTitle("VoxFox API")
				.WithTheme(ScalarTheme.DeepSpace)
				.ForceDarkMode()
				.WithOpenApiRoutePattern("/openapi/{documentName}.json")
				.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
				.AddPreferredSecuritySchemes("Bearer")
				.DisableAgent();
		});

		return app;
	}
}
