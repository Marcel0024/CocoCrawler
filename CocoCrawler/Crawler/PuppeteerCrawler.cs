using CocoCrawler.Job;
using CocoCrawler.Job.PageBrowserActions;
using CocoCrawler.Job.PageTasks;
using CocoCrawler.Parser;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;

namespace CocoCrawler.Crawler;

public class PuppeteerCrawler : ICrawler
{
    private ILogger? Logger { get; set; }
    private IParser? Parser { get; set; }

    public virtual async Task<CrawlResult> Crawl(IPage browserTab, PageCrawlJob currentPageJob)
    {
        Logger?.LogInformation("Getting page {Url}", currentPageJob.Url);

        await browserTab.GoToAsync(currentPageJob.Url);

        await ExecutePageActions(browserTab, currentPageJob.BrowserActions);

        var newJobs = new List<PageCrawlJob>();
        var jArray = new JArray();

        await Parse(currentPageJob, await browserTab.GetContentAsync(), newJobs, jArray);

        Logger?.LogInformation("Finished getting {Url}. Total new jobs: {totaljobs}. Total new objects: {totalobj}", currentPageJob.Url, newJobs.Count, jArray.Count);

        return new CrawlResult([.. newJobs], jArray);
    }

    protected virtual async Task ExecutePageActions(IPage page, PageActions? browserActions)
    {
        if (browserActions == null)
        {
            return;
        }

        foreach (var action in browserActions.Actions)
        {
            switch (action.Type)
            {
                case PageActionType.ScrollToEnd:
                    await page.EvaluateExpressionAsync("window.scrollTo(0, document.body.scrollHeight);");
                    break;
                case PageActionType.Wait:
                    await Task.Delay(Convert.ToInt32(action.Parameters));
                    break;
                case PageActionType.Click:
                    await page.ClickAsync(action.Parameters);
                    break;
            }
        }
    }

    protected virtual async Task Parse(PageCrawlJob job, string html, List<PageCrawlJob> newJobs, JArray jArray)
    {
        ArgumentNullException.ThrowIfNull(Parser);

        await Parser.Init(html);

        foreach (var task in job.Tasks)
        {
            switch (task)
            {
                case CrawlPageOpenLinksTask openLinks:
                    HandleOpenLinksTask(openLinks, job, newJobs);
                    break;
                case CrawlPagePaginateTask paginate:
                    HandlePaginateTask(paginate, job, newJobs);
                    break;
                case CrawlPageExtractObjectTask scrape:
                    HandleExtractObject(scrape, job.Url, jArray);
                    break;
                case CrawlPageExtractListTask scrapeList:
                    HandleExtractList(scrapeList, jArray);
                    break;
                default:
                    throw new NotImplementedException("Task not implemented");
            }
        }
    }

    private void HandleExtractList(CrawlPageExtractListTask scrapeList, JArray jArray)
    {
        var jArrayResult = Parser!.ExtractList(scrapeList);

        foreach (var obj in jArrayResult.Cast<JObject>())
        {
            jArray.Add(obj);
        }
    }

    private void HandleExtractObject(CrawlPageExtractObjectTask scrape, string url, JArray jArray)
    {
        var parsedObject = Parser!.ExtractObject(scrape);

        parsedObject.AddFirst(new JProperty("Url", url));

        jArray.Add(parsedObject);
    }

    protected virtual void HandlePaginateTask(CrawlPagePaginateTask paginate, PageCrawlJob job, List<PageCrawlJob> newJobs)
    {
        var urls = Parser!.ParseForLinks(paginate.PaginationSelector);

        Logger?.LogDebug("Paginate selector {Count} Urls found in paginate task.", urls.Length);

        var newPages = urls.Select(url => new PageCrawlJob(url, [.. job.Tasks], [.. job.Outputs], job.BrowserActions));

        newJobs.AddRange(newPages);
    }

    protected virtual void HandleOpenLinksTask(CrawlPageOpenLinksTask openLinks, PageCrawlJob job, List<PageCrawlJob> newJobs)
    {
        var urls = Parser!.ParseForLinks(openLinks.OpenLinksSelector);

        Logger?.LogDebug("OpenLinks selector returned {Count} Urls found in openLinks task.", urls.Length);

        foreach (var url in urls)
        {
            var newPageBuilder = openLinks.JobBuilder;

            newPageBuilder.WithUrl(url);
            newPageBuilder.AddOutput([.. job.Outputs]);
            newPageBuilder.WithTasks(job.Tasks.Where(t => t is CrawlPageExtractObjectTask).ToArray());

            var newPage = openLinks.JobBuilder.Build();

            newJobs.Add(newPage);
        }
    }

    public void WithParser(IParser parser)
    {
        Parser = parser;
    }

    public void WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger<PuppeteerCrawler>();
    }
}

