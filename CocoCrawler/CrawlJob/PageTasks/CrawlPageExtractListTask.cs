using CocoCrawler.Parser;

namespace CocoCrawler.Job.PageTasks;

public class CrawlPageExtractListTask(string ContentContainersSelector, List<CssSelector> selectors) : IPageCrawlTask
{
    public string ContentContainersSelector { get; init; } = ContentContainersSelector;
    public List<CssSelector> Selectors { get; init; } = selectors;
}
