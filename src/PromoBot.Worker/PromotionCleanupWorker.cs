using PromoBot.Application.Interfaces;

namespace PromoBot.Worker;

public class PromotionCleanupWorker(IServiceScopeFactory scopeFactory,
    ILogger<PromotionCleanupWorker> logger) : BackgroundService
{
    private const int RetentionDays = 1;
    
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Promotion cleanup worker started. Cleaning up promotions older than " +
                              "{RetentionDays} days every {Interval}.", RetentionDays, Interval);

        await WaitUntilNextExecutionAsync(targetHour: 9, stoppingToken);
        
        using var timer = new PeriodicTimer(Interval);

        do
        {
            await RunCleanupAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunCleanupAsync(CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Starting promotion cleanup...");
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IPromotionRepository>();
            
            var cutOffDate = DateTime.UtcNow.AddDays(-RetentionDays);
            var deletedCount = await repository.DeleteOlderPromotionThanAsync(cutOffDate, ct);
            
            logger.LogInformation($"Promotion cleanup finished.{deletedCount} promotions deleted.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred during promotion cleanup.");
        }
    }

    private static async Task WaitUntilNextExecutionAsync(long targetHour, CancellationToken ct)
    {
        var now = DateTime.Now;
        var nextRun = now.Date.AddHours(targetHour);
        
        if (now >= nextRun)
            nextRun = nextRun.AddDays(1);
        
        var delay = nextRun - now;
        
        await Task.Delay(delay, ct);
    }
}