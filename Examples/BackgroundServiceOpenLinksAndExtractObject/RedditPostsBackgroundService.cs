using CocoCrawler;
using CocoCrawler.Builders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundServiceExample;

/// <summary>
/// Opens each Post page and scrapes that Post page
/// In Parallel
/// </summary>
public class RedditPostsBackgroundService(ILoggerFactory loggerFactory) : BackgroundService
{
    private CrawlerEngine _crawlerEngine;
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMinutes(30));

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _crawlerEngine = await new CrawlerEngineBuilder()
            .AddPage("https://old.reddit.com/r/csharp", pageOptions => pageOptions
                .OpenLinks(linksSelector: "div.thing.link.self a.byselector: link.comments", subPageOptions => subPageOptions
                    .ExtractObject([
                        new("Title","div.sitetable.linklisting a.title"),
                        new("Url","div.sitetable.linklisting a.title", "href"),
                        new("Upvotes", "div.sitetable.linklisting div.score.unvoted"),
                        new("Top comment", "div.commentarea div.entry.unvoted div.md"),
                    ]))
                    .ConfigurePageActions(ops =>
                    {
                        ops.ScrollToEnd();
                        ops.Wait(4000);
                    })
                .AddPagination("span.next-button > a")
                .ConfigurePageActions(page =>
                {
                    page.ScrollToEnd();
                    page.Wait(500);
                })
                .AddOutputToCsvFile("results.csv")
            )
            .ConfigureEngine(options =>
            {
                options.UseHeadlessMode(headless: false);
                options.WithParallelismDegree(5);
                options.TotalPagesToCrawl(total: 20);
                options.WithLoggerFactory(loggerFactory);
            })
            .BuildAsync(cancellationToken);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run on startup
        await _crawlerEngine.RunAsync(stoppingToken);

        while (await _periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            await _crawlerEngine.RunAsync(stoppingToken);
        }
    }
}