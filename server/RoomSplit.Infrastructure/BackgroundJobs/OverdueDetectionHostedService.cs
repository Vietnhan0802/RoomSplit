using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoomSplit.Core.Interfaces;

namespace RoomSplit.Infrastructure.BackgroundJobs;

public class OverdueDetectionHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OverdueDetectionHostedService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public OverdueDetectionHostedService(
        IServiceProvider serviceProvider,
        ILogger<OverdueDetectionHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OverdueDetectionHostedService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Checking for overdue assignments...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                    await taskService.MarkOverdueAssignmentsAsync();
                }

                _logger.LogInformation(
                    "Overdue detection completed. Next check in {Interval}",
                    _interval);

                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when service is stopping
                _logger.LogInformation("OverdueDetectionHostedService is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during overdue detection.");
                // Wait 10 minutes before retrying on error
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }
}
