using CocoCrawler.Crawler;
using CocoCrawler.CrawlJob;
using CocoCrawler.Exceptions;
using CocoCrawler.Job;
using CocoCrawler.Outputs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System.Collections.Immutable;

namespace CocoCrawler;

public class CrawlerEngine(EngineSettings settings, ImmutableArray<PageCrawlJob> jobs)
{
    private readonly ILogger? _logger = settings.LoggerFactory?.CreateLogger<CrawlerEngine>();

    public virtual async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await using var browser = await DownloadAndLaunchBrowser(settings);
        await settings.Scheduler.Init(jobs, cancellationToken);

        try
        {
            if (settings.DisableParallelism)
            {
                // 1 browser with 1 tab for all jobs
                using var page = await browser.NewPageAsync();
                await AddUserAgent(page, settings.UserAgent);
                await AddCookies(page, settings.Cookies);

                await foreach (var job in settings.Scheduler.GetAll(cancellationToken))
                {
                    await CrawlPage(page, job, settings, cancellationToken);
                }
            }
            else
            {
                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = settings.ParallelismDegree
                };

                // 1 browser with a tab for each job in parallel
                await Parallel.ForEachAsync(settings.Scheduler.GetAll(cancellationToken), parallelOptions, async (job, token) =>
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
            _logger?.LogInformation("Crawl Finished. {ex}. To Increase the crawl limit call .ConfigureEngine(o => o.TotalPagesToCrawl(...))", ex.Message);
            return;
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("Cancelled task. Stopping engine.");
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

    protected virtual async Task AddCookies(IPage page, ImmutableArray<Cookie> cookies)
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

        await engine.Scheduler.Add(jobs.ToImmutableArray(), token);
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

    protected virtual void AddUrlToHistoryAndCheckLimit(string url, HistoryTracker historyTracker, int maxPagesToCrawl)
    {
        if (historyTracker.GetVisitedLinksCount() >= maxPagesToCrawl)
        {
            throw new CocoCrawlerPageLimitReachedException($"Max pages to crawl limit reached at {maxPagesToCrawl}.");
        }

        historyTracker.AddUrl(url);
    }

    protected virtual async Task<IBrowser> DownloadAndLaunchBrowser(EngineSettings settings)
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        var launchOptions = new LaunchOptions()
        {
            Headless = settings.IsHeadless
        };

        return await Puppeteer.LaunchAsync(launchOptions);
    }
}