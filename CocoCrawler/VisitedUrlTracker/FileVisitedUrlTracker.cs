namespace CocoCrawler.VisitedUrlTracker;

public class FileVisitedUrlTracker(string fileFullPath, bool cleanOnStart) : InMemoryVisitedUrlTracker
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public override async Task Initialize(CancellationToken cancellationToken)
    {
        await base.Initialize(cancellationToken);

        if (cleanOnStart && File.Exists(fileFullPath))
        {
            File.Delete(fileFullPath);
        }

        var dir = Path.GetDirectoryName(fileFullPath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (File.Exists(fileFullPath))
        {
            var visitedUrls = await File.ReadAllLinesAsync(fileFullPath, cancellationToken);
            foreach (var visitedUrl in visitedUrls)
            {
                await AddVisitedUrl(visitedUrl, cancellationToken);
            }
        }
    }

    public override async Task AddVisitedUrl(string visitedLink, CancellationToken cancellationToken)
    {
        await base.AddVisitedUrl(visitedLink, cancellationToken);

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            await File.AppendAllLinesAsync(fileFullPath, [visitedLink], cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
