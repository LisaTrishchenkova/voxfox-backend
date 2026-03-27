using Microsoft.OpenApi;

namespace VoxFox.Extensions;

public static class SwaggerExtensions
{
	public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
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

	public static IApplicationBuilder UseSwaggerWithUi(this IApplicationBuilder app)
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

		return app;
	}
}
