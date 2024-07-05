using CocoCrawler.Job.PageBrowserActions;

namespace CocoCrawler.Job.PageTasks;

public class CrawlPagePaginateTask(string paginationSelector, PageActions? pageActions = null) 
    : IPageCrawlTask
{
    public string PaginationSelector { get; init; } = paginationSelector;
    public PageActions? PageActions { get; init; } = pageActions;
}
