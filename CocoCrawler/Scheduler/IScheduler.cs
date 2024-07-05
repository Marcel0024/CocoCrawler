using CocoCrawler.Job;
using System.Collections.Immutable;

namespace CocoCrawler.Scheduler;

public interface IScheduler
{
    IAsyncEnumerable<PageCrawlJob> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(PageCrawlJob job, CancellationToken cancellationToken);
    Task AddAsync(ImmutableArray<PageCrawlJob> jobs, CancellationToken cancellationToken);
}
