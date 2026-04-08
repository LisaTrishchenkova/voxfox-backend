using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;
using VoxFox.Models.Entities;

namespace VoxFox.Services
{
	public class MigrationHostedService : IHostedService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<MigrationHostedService> _logger;

		public MigrationHostedService(IServiceProvider serviceProvider, ILogger<MigrationHostedService> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			var retryPolicy = Policy
				.Handle<System.Exception>(ex => ex is DbUpdateException || ex is TimeoutException || ex is NpgsqlException)
				.WaitAndRetryAsync(
					retryCount: 3,
					sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(1),
					onRetry: (exception, timeSpan, retryCount, context) =>
					{
						_logger.LogWarning(
							exception,
							$"Attempt {retryCount} of 3 failed with exception {exception.GetType().Name}. " +
							$"Waiting {timeSpan.TotalSeconds} seconds before next retry."
						);
					}
				);

			await retryPolicy.ExecuteAsync(async () =>
			{
				using (var scope = _serviceProvider.CreateScope())
				{
					var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

					var applied = await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);
					var all = dbContext.Database.GetMigrations();
					var pending = all.Except(applied).ToList();

					if (pending.Any())
					{
						_logger.LogInformation("Applying pending migrations:");
						foreach (var m in pending)
						{
							_logger.LogInformation($"→ {m}");
						}
					}
					else
					{
						_logger.LogInformation("No pending migrations.");
					}

					await dbContext.Database.MigrateAsync(cancellationToken);
					_logger.LogInformation("Migrations applied successfully.");
				}
			});

		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("MigrationHostedService is stopping.");
			return Task.CompletedTask;
		}
	}
}
