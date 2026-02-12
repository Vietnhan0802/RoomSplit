using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoomSplit.Core.Interfaces;

namespace RoomSplit.Infrastructure.BackgroundJobs;

public class TaskGenerationHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskGenerationHostedService> _logger;

    public TaskGenerationHostedService(
        IServiceProvider serviceProvider,
        ILogger<TaskGenerationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TaskGenerationHostedService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Calculate delay until next midnight UTC
                var now = DateTime.UtcNow;
                var nextMidnight = now.Date.AddDays(1);
                var delay = nextMidnight - now;

                _logger.LogInformation(
                    "Next task generation scheduled at {NextRun} (in {Delay})",
                    nextMidnight,
                    delay);

                await Task.Delay(delay, stoppingToken);

                // Generate assignments for all active templates
                using (var scope = _serviceProvider.CreateScope())
                {
                    var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

                    _logger.LogInformation("Generating assignments for all active templates...");
                    await taskService.GenerateAllActiveTemplatesAsync();
                    _logger.LogInformation("Task generation completed successfully.");
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when service is stopping
                _logger.LogInformation("TaskGenerationHostedService is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during task generation.");
                // Wait 1 hour before retrying on error
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
