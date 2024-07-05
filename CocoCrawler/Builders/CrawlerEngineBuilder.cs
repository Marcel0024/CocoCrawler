using CocoCrawler.Exceptions;
using CocoCrawler.Job;
using System.Collections.Immutable;

namespace CocoCrawler.Builders;

/// <summary>
/// Builder class for creating a CrawlerEngine instance.
/// </summary>
public class CrawlerEngineBuilder
{
    private EngineSettingsBuilder EngineSettingsBuilder { get; } = new();
    private PageCrawlJobBuilder[] CrawlPages { get; set; } = [];

    /// <summary>
    /// Adds a single page to the crawler engine.
    /// </summary>
    /// <param name="url">The URL of the page to crawl.</param>
    /// <param name="options">The action to configure the page crawl job.</param>
    /// <returns>The CrawlerEngineBuilder instance.</returns>
    public CrawlerEngineBuilder AddPage(string url, Action<PageCrawlJobBuilder> options)
    {
        return AddPages([url], options);
    }

    /// <summary>
    /// Adds multiple pages to the crawler engine.
    /// </summary>
    /// <param name="urls">The URLs of the pages to crawl.</param>
    /// <param name="options">The action to configure the page crawl jobs.</param>
    /// <returns>The CrawlerEngineBuilder instance.</returns>
    public CrawlerEngineBuilder AddPages(string[] urls, Action<PageCrawlJobBuilder> options)
    {
        var firstPage = new PageCrawlJobBuilder(urls[0]);
        options(firstPage);

        CrawlPages = urls.Select(url =>
        {
            var pageJob = new PageCrawlJobBuilder(url);

            options(pageJob);

            // Temp? Make sure all outputs are using the same instance
            pageJob.ReplaceOutputs([.. firstPage.Outputs]);

            return pageJob;
        }).ToArray();

        return this;
    }

    /// <summary>
    /// Configures the engine settings for the crawler engine.
    /// </summary>
    /// <param name="options">The action to configure the engine settings.</param>
    /// <returns>The CrawlerEngineBuilder instance.</returns>
    public CrawlerEngineBuilder ConfigureEngine(Action<EngineSettingsBuilder> options)
    {
        options(EngineSettingsBuilder);

        return this;
    }

    /// <summary>
    /// Builds the crawler engine asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The built CrawlerEngine instance.</returns>
    public async Task<CrawlerEngine> BuildAsync(CancellationToken cancellationToken = default)
    {
        if (CrawlPages.Length == 0)
        {
            throw new CocoCrawlerBuilderException($"At least one Page is required to build the engine. Try calling .{nameof(AddPage)}() to add pages.");
        }

        var engineSettings = EngineSettingsBuilder.Build();
        var jobs = CrawlPages.Select(cp => cp.Build()).ToImmutableArray();

        await engineSettings.Scheduler.AddAsync(jobs, cancellationToken);

        await InitializeOutputs(jobs, cancellationToken);

        return new CrawlerEngine(engineSettings);
    }

    private static async Task InitializeOutputs(ImmutableArray<PageCrawlJob> jobs, CancellationToken token)
    {
        var tasks = jobs.SelectMany(j => j.Outputs.Select(x => x.Initiaize(token)));

        await Task.WhenAll(tasks);
    }
}
