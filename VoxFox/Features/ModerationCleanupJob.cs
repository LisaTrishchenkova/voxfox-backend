using VoxFox.Interfaces.Moderation;

namespace VoxFox.Features;

public class ModerationCleanupJob : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<ModerationCleanupJob> _logger;
	private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
	private static readonly TimeSpan ClaimTimeout = TimeSpan.FromHours(24);

	public ModerationCleanupJob(IServiceScopeFactory scopeFactory, ILogger<ModerationCleanupJob> logger)
	{
		_scopeFactory = scopeFactory;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = _scopeFactory.CreateScope();
				var service = scope.ServiceProvider.GetRequiredService<IModerationService>();
				await service.ReleaseStaleClaimsAsync(ClaimTimeout);
				_logger.LogInformation("Завершена очистка модерации.");
			}
			catch (System.Exception ex)
			{
				_logger.LogError(ex, "Не удалось выполнить очистку модерации.");
			}

			await Task.Delay(Interval, stoppingToken);
		}
	}
}
