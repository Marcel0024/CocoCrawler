using CocoCrawler.Crawler;
using CocoCrawler.Exceptions;
using CocoCrawler.Job;
using CocoCrawler.Outputs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System.Collections.Immutable;
using System.Net;

namespace CocoCrawler;

public class CrawlerEngine(EngineSettings settings)
{
    private readonly ILogger? _logger = settings.LoggerFactory?.CreateLogger<CrawlerEngine>();

    public virtual async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = settings.ParallelismDegree
        };

        await DownloadBrowser();
        await using var browser = await LaunchBrowser(settings);

        try
        {
            if (settings.DisableParallelism)
            {
                // Use 1 browser with 1 tab for all jobs
                using var page = await browser.NewPageAsync();
                await AddUserAgent(page, settings.UserAgent);
                await AddCookies(page, settings.Cookies);

                await foreach (var job in settings.Scheduler.GetAllAsync(cancellationToken))
                {
                    await CrawlPage(page, job, settings, cancellationToken);
                }
            }
            else
            {
                // Use 1 browser with a tab for each job in parallel
                await Parallel.ForEachAsync(settings.Scheduler.GetAllAsync(cancellationToken), parallelOptions, async (job, token) =>
                {
                    using var page = await browser.NewPageAsync();
                    await AddUserAgent(page, settings.UserAgent);
                    await AddCookies(page, settings.Cookies);

                    await CrawlPage(page, job, settings, token);
                });
            }
        }
        catch (CocoCrawlerPageLimitReachedException ex)
        {
            _logger?.LogInformation("Crawl Finished. {ex}. To Increase the limit call .ConfigureEngine(o => o.TotalPagesToCrawl(...))", ex.Message);
            return;
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("CancellationToken is cancelled, stopping engine.");
            return;
        }
        catch (Exception ex)
        {
            _logger?.LogCritical(ex, "An unexpect error occured. Stopping engine.");
            throw;
        }
    }

    protected virtual async Task AddUserAgent(IPage page, string? userAgent)
    {
        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            await page.SetUserAgentAsync(userAgent);
        }
    }

    protected virtual async Task AddCookies(IPage page, Cookie[] cookies)
    {
        if (cookies.Length > 0)
        {
            await page.SetCookieAsync(cookies.Select(c => new CookieParam()
            {
                Name = c.Name,
                Value = c.Value,
                Domain = c.Domain,
                Path = c.Path
            }).ToArray());
        }
    }

    protected virtual async Task CrawlPage(IPage page, PageCrawlJob job, EngineSettings engine, CancellationToken token)
    {
        AddUrlToHistoryAndCheckLimit(job.Url, engine.HistoryTracker, engine.MaxPagesToCrawl);

        var result = await engine.Crawler.Crawl(page, job);

        await HandleNewJobs(result.NewJobs, engine, token);
        await HandleParsedResults(result.ScrapedData, job.Outputs, token);
    }

    protected virtual async Task HandleNewJobs(IList<PageCrawlJob> newJobs, EngineSettings engine, CancellationToken token)
    {
        var jobs = newJobs.Where(ncj => !engine.IgnoreUrls.Any(iu => iu == ncj.Url));

        await engine.Scheduler.AddAsync(jobs.ToImmutableArray(), token);
    }

    protected virtual async Task HandleParsedResults(JArray jArray, ImmutableArray<ICrawlOutput> outputs, CancellationToken token)
    {
        foreach (var output in outputs)
        {
            foreach (var obj in jArray.Cast<JObject>())
            {
                await output.WriteAsync(obj, token);
            }
        }
    }

    protected virtual async Task<IBrowser> LaunchBrowser(EngineSettings engineSettings)
    {
        var launchOptions = new LaunchOptions()
        {
            Headless = engineSettings.IsHeadless
        };

        return await Puppeteer.LaunchAsync(launchOptions);
    }

    protected virtual void AddUrlToHistoryAndCheckLimit(string url, HistoryTracker historyTracker, int maxPagesToCrawl)
    {
        if (historyTracker.GetVisitedLinksCount() >= maxPagesToCrawl)
        {
            throw new CocoCrawlerPageLimitReachedException($"Max pages to crawl limit reached at {maxPagesToCrawl}.");
        }

        historyTracker.AddUrl(url);
    }

    protected virtual async Task DownloadBrowser()
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
    }
}