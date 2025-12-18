using System.Threading.Channels;
using Application.Abstractions.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

internal static class BackgroundJobQueue
{
    public static readonly Channel<Func<IServiceProvider, CancellationToken, Task>> Queue =
        Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>();
}

internal sealed class BackgroundJobService : IBackgroundJobService
{
    public string EnqueueJob<T>(Func<T, CancellationToken, Task> job, CancellationToken cancellationToken = default) where T : class
    {
        string jobId = Guid.NewGuid().ToString("N");

        BackgroundJobQueue.Queue.Writer.TryWrite(async (serviceProvider, ct) =>
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            T service = scope.ServiceProvider.GetRequiredService<T>();
            await job(service, ct);
        });

        return jobId;
    }

    public void EnqueueFireAndForget(Func<CancellationToken, Task> job)
    {
        BackgroundJobQueue.Queue.Writer.TryWrite(async (_, ct) => await job(ct));
    }
}

internal sealed class BackgroundJobWorker(
    IServiceProvider serviceProvider,
    ILogger<BackgroundJobWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Background job worker started");

        await foreach (Func<IServiceProvider, CancellationToken, Task> job in BackgroundJobQueue.Queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await job(serviceProvider, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing background job");
            }
        }

        logger.LogInformation("Background job worker stopped");
    }
}
