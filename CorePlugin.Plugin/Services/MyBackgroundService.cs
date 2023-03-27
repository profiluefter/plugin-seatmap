using Microsoft.Extensions.Hosting;

namespace CorePlugin.Plugin.Services;

internal class MyBackgroundService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Executing MyBackgroundService");
        return Task.CompletedTask;
    }
}
