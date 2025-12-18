namespace Application.Abstractions.BackgroundJobs;

public interface IBackgroundJobService
{
    string EnqueueJob<T>(Func<T, CancellationToken, Task> job, CancellationToken cancellationToken = default) where T : class;

    void EnqueueFireAndForget(Func<CancellationToken, Task> job);
}
