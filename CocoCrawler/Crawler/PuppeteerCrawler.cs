using AngleSharp;
using AngleSharp.Dom;
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
    private IParser? Parser { get; set; }
    private ILogger? Logger { get; set; }

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
            }
        }
    }

    protected virtual async Task Parse(PageCrawlJob job, string html, List<PageCrawlJob> newJobs, JArray jArray)
    {
        ArgumentNullException.ThrowIfNull(Parser, nameof(Parser));

        var doc = await GetDocument(html);

        foreach (var task in job.Tasks)
        {
            switch (task)
            {
                case CrawlPageOpenLinksTask openLinks:
                    {
                        var urls = Parser.GetUrlsFromSelector(doc, openLinks.OpenLinksSelector);

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
                    break;
                case CrawlPagePaginateTask paginate:
                    {
                        var urls = Parser.GetUrlsFromSelector(doc, paginate.PaginationSelector);

                        Logger?.LogDebug("Paginate selector {Count} Urls found in paginate task.", urls.Length);

                        var newPages = urls.Select(url => new PageCrawlJob(url, [.. job.Tasks], [.. job.Outputs], paginate.PageActions));

                        newJobs.AddRange(newPages);
                    }
                    break;
                case CrawlPageExtractObjectTask scrape:
                    {
                        jArray.Add(Parser.ExtractObject(doc, scrape));
                    }
                    break;
                case CrawlPageExtractListTask scrapeList:
                    {
                        var jArrayResult = Parser.ExtractList(doc, scrapeList);

                        foreach (var obj in jArrayResult.Cast<JObject>())
                        {
                            jArray.Add(obj);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException("Task not implemented");
            }
        }
    }

    protected virtual async Task<IDocument> GetDocument(string html)
    {
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);

        var document = await context.OpenAsync(req => req.Content(html));

        return document;
    }

    public void WithParser(IParser parser)
    {
        Parser = parser;
    }

    public void WithLoggerFactory(ILoggerFactory? loggerFactory)
    {
        Logger = loggerFactory?.CreateLogger<PuppeteerCrawler>();
    }
}

