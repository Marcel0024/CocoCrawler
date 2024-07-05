using CocoCrawler.Parser;

namespace CocoCrawler.Job.PageTasks;

public class CrawlPageExtractObjectTask(List<CssSelector> selectors) : IPageCrawlTask
{
    public List<CssSelector> Selectors { get; init; } = selectors;
}
