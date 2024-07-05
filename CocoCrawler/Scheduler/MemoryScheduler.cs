using CocoCrawler.Job;
using System.Collections.Immutable;
using System.Threading.Channels;

namespace CocoCrawler.Scheduler;

public class MemoryScheduler : IScheduler
{
    private readonly Channel<PageCrawlJob> _jobChannel = Channel.CreateUnbounded<PageCrawlJob>();
    private readonly Timer _timer;
    private readonly TimeSpan _completeAfterLastJob = TimeSpan.FromMinutes(2);

    public MemoryScheduler()
    {
        _timer = new Timer(CompleteIfNoJobReceived, null, _completeAfterLastJob, _completeAfterLastJob);
    }

    public IAsyncEnumerable<PageCrawlJob> GetAllAsync(CancellationToken cancellationToken)
    {
        return _jobChannel.Reader.ReadAllAsync(cancellationToken);
    }

    public async Task AddAsync(PageCrawlJob job, CancellationToken cancellationToken)
    {
        await _jobChannel.Writer.WriteAsync(job, cancellationToken);
        ResetTimer();
    }

    public async Task AddAsync(ImmutableArray<PageCrawlJob> jobs, CancellationToken cancellationToken)
    {
        foreach (var job in jobs)
        {
            await AddAsync(job, cancellationToken);
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
