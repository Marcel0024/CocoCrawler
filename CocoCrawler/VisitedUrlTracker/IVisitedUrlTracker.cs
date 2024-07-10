namespace CocoCrawler.VisitedUrlTracker;

public interface IVisitedUrlTracker
{
    Task Initialize(CancellationToken cancellationToken);
    Task AddVisitedUrl(string visitedLink, CancellationToken cancellationToken);
    Task<List<string>> GetVisitedUrls(CancellationToken cancellationToken);
    Task<int> GetVisitedUrlsCount(CancellationToken cancellationToken);
}
