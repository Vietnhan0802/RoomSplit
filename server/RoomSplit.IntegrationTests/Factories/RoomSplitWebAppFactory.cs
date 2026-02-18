using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoomSplit.Infrastructure.BackgroundJobs;
using RoomSplit.Infrastructure.Data;

namespace RoomSplit.IntegrationTests.Factories;

public class RoomSplitWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Remove background services
            var hostedServices = services.Where(d =>
                d.ImplementationType == typeof(TaskGenerationHostedService) ||
                d.ImplementationType == typeof(OverdueDetectionHostedService))
                .ToList();
            foreach (var svc in hostedServices)
                services.Remove(svc);

            // Add InMemory database
            var dbName = $"RoomSplitTestDb_{Guid.NewGuid()}";
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
            });

            // Build the service provider and ensure the database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
