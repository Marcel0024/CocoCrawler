namespace CocoCrawler.Job.PageTasks;

public class CrawlPagePaginateTask(string paginationSelector) 
    : IPageCrawlTask
{
    public string PaginationSelector { get; init; } = paginationSelector;
}
