using CocoCrawler.Job;
using System.Collections.Immutable;
using System.Threading.Channels;

namespace CocoCrawler.Scheduler;

public class InMemoryScheduler : IScheduler
{
    private Channel<PageCrawlJob> _jobChannel = Channel.CreateUnbounded<PageCrawlJob>();
    private readonly Timer _timer;
    private readonly TimeSpan _completeAfterLastJob;

    public InMemoryScheduler(int totalSecondsTimeoutAfterJob = 120)
    {
        _completeAfterLastJob = TimeSpan.FromSeconds(totalSecondsTimeoutAfterJob);
        _timer = new Timer(CompleteIfNoJobReceived, null, _completeAfterLastJob, _completeAfterLastJob);
    }

    public virtual IAsyncEnumerable<PageCrawlJob> GetAll(CancellationToken cancellationToken)
    {
        return _jobChannel.Reader.ReadAllAsync(cancellationToken);
    }

    public virtual async Task Add(PageCrawlJob job, CancellationToken cancellationToken)
    {
        await _jobChannel.Writer.WriteAsync(job, cancellationToken);
        ResetTimer();
    }

    public virtual async Task Add(ImmutableArray<PageCrawlJob> jobs, CancellationToken cancellationToken)
    {
        foreach (var job in jobs)
        {
            await Add(job, cancellationToken);
        }
    }

    public virtual async Task Initialize(ImmutableArray<PageCrawlJob> jobs, CancellationToken cancellationToken)
    {
        _jobChannel = Channel.CreateUnbounded<PageCrawlJob>();

        foreach (var job in jobs)
        {
            await Add(job, cancellationToken);
        }
    }

    private void ResetTimer()
    {
        _timer.Change(_completeAfterLastJob, _completeAfterLastJob);
    }

    private void CompleteIfNoJobReceived(object? state)
    {
        Complete();
    }

    private void Complete()
    {
        _jobChannel.Writer.Complete();
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
    }
}
