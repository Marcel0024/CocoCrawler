using CocoCrawler;
using CocoCrawler.Builders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundServiceExample;

/// <summary>
/// Only scrapes the the listing pages (does not open pages)
/// </summary>
public class RedditListingBackgroundService(ILoggerFactory loggerFactory) : BackgroundService
{
    private CrawlerEngine _crawlerEngine;
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMinutes(30));

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _crawlerEngine = await new CrawlerEngineBuilder()
            .AddPages(["https://old.reddit.com/r/csharp", "https://old.reddit.com/r/dotnet"], pageOptions => pageOptions
                .ExtractList(containersSelector: "div.thing.link.self", [
                    new("Title","a.title"),
                    new("Upvotes", "div.score.unvoted"),
                    new("Datetime", "time", "datetime"),
                    new("Total Comments","a.comments"),
                    new("Url","a.title", "href")
                ])
                .AddPagination("span.next-button > a.not-exist", newPage => newPage.ScrollToEnd())
                .AddOutputToConsole()
                .AddOutputToCsvFile("results.csv")
            )
            .ConfigureEngine(options =>
            {
                options.UseHeadlessMode(false);
                options.TotalPagesToCrawl(total: 10);
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