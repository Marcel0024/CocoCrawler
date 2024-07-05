using System.Collections.Immutable;

namespace CocoCrawler.Crawler;

public class HistoryTracker
{
    private ImmutableHashSet<string> visitedUrls = [];

    public void AddUrl(string visitedLink)
    {
        ImmutableInterlocked.Update(ref visitedUrls, set => set.Add(visitedLink));
    }

    public List<string> GetVisitedLinks()
    {
        return [.. visitedUrls];
    }

    public int GetVisitedLinksCount()
    {
        return visitedUrls.Count;
    }
}
