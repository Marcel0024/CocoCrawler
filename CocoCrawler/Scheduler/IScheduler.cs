using CocoCrawler.Job;
using System.Collections.Immutable;

namespace CocoCrawler.Scheduler;

public interface IScheduler
{
    IAsyncEnumerable<PageCrawlJob> GetAll(CancellationToken cancellationToken);
    Task Add(PageCrawlJob job, CancellationToken cancellationToken);
    Task Add(ImmutableArray<PageCrawlJob> jobs, CancellationToken cancellationToken);
    Task Initialize(ImmutableArray<PageCrawlJob> jobs, CancellationToken cancellationToken);
}
