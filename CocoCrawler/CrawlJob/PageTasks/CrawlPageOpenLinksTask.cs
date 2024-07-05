using CocoCrawler.Builders;
using CocoCrawler.Job.PageBrowserActions;

namespace CocoCrawler.Job.PageTasks;

public class CrawlPageOpenLinksTask(string paginationSelector, PageCrawlJobBuilder builder, PageActions? pageActions = null) : IPageCrawlTask
{
    public string OpenLinksSelector { get; init; } = paginationSelector;
    public PageActions? PageActions { get; init; } = pageActions;
    public PageCrawlJobBuilder JobBuilder { get; init; } = builder;
}

