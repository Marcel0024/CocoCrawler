using System.Collections.Immutable;

namespace CocoCrawler.VisitedUrlTracker;

public class InMemoryVisitedUrlTracker : IVisitedUrlTracker
{
    private ImmutableHashSet<string> _urls = [];

    public virtual Task Initialize(CancellationToken _)
    {
        _urls = [];
        return Task.CompletedTask;
    }

    public virtual Task AddVisitedUrl(string url, CancellationToken _)
    {
        ImmutableInterlocked.Update(ref _urls, set => set.Add(url));

        return Task.CompletedTask;
    }

    public virtual Task<List<string>> GetVisitedUrls(CancellationToken _)
    {
        return Task.FromResult(_urls.ToList());
    }

    public virtual Task<int> GetVisitedUrlsCount(CancellationToken _)
    {
        return Task.FromResult(_urls.Count);
    }
}
