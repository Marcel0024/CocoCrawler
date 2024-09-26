using AngleSharp.Dom;
using CocoCrawler.Builders;
using CocoCrawler.Job.PageBrowserActions;

namespace CocoCrawler.Job.PageTasks;

public class CrawlPageOpenLinksTask : IPageCrawlTask
{
    public string OpenLinksSelector { get; init; }
    public PageActions? PageActions { get; init; }
    public Func<IElement, string?>? LinkProcessor { get; }
    public PageCrawlJobBuilder JobBuilder { get; init; }

    public CrawlPageOpenLinksTask(string linksSelector, PageCrawlJobBuilder builder, PageActions? pageActions = null)
    {
        OpenLinksSelector = linksSelector;
        PageActions = pageActions;
        JobBuilder = builder;
    }

    public CrawlPageOpenLinksTask(string linksSelector, PageCrawlJobBuilder builder, PageActions? pageActions = null, Func<IElement, string?>? linkProcessor = null)
    {
        OpenLinksSelector = linksSelector;
        PageActions = pageActions;
        LinkProcessor = linkProcessor;
        JobBuilder = builder;
    }
}

