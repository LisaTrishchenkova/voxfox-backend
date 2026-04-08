using Microsoft.AspNetCore.Diagnostics;

namespace VoxFox.Extensions;

public static class ExceptionHandlerExtensions
{
	public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app, IWebHostEnvironment env)
	{
		app.UseExceptionHandler(appError =>
		{
			appError.Run(async context =>
			{
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				context.Response.ContentType = "application/json";

				var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
				if (contextFeature is null) return;

				var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
				var error = contextFeature.Error;

				logger.LogError(error, "Unhandled exception occurred. TraceId: {TraceId}",
					context.TraceIdentifier);

				var response = new
				{
					error = "An error occurred while processing your request",
					traceId = context.TraceIdentifier,

					message = env.IsDevelopment() ? error.Message : null,
					stackTrace = env.IsDevelopment() ? error.StackTrace : null,
					type = env.IsDevelopment() ? error.GetType().Name : null
				};

				await context.Response.WriteAsJsonAsync(response);
			});
		});

		return app;
	}
}
