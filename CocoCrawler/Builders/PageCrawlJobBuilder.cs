using AngleSharp.Css;
using AngleSharp.Css.Parser;
using CocoCrawler.CrawlOutputs;
using CocoCrawler.Exceptions;
using CocoCrawler.Job;
using CocoCrawler.Job.PageBrowserActions;
using CocoCrawler.Job.PageTasks;
using CocoCrawler.Outputs;
using CocoCrawler.Parser;
using System.Collections.Immutable;

namespace CocoCrawler.Builders;

/// <summary>
/// Builder class for creating a page crawl job.
/// </summary>
public class PageCrawlJobBuilder
{
    private string? Url { get; set; }
    private List<IPageCrawlTask> Tasks { get; set; } = [];
    private PageActionsBuilder? PageActionsBuilder { get; set; }
    internal List<ICrawlOutput> Outputs { get; private set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="PageCrawlJobBuilder"/> class with the specified URL.
    /// </summary>
    /// <param name="url">The URL of the page to crawl.</param>
    public PageCrawlJobBuilder(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            throw new CocoCrawlerBuilderException($"{url} is not a valid URI.");
        }

        Url = url;
    }

    /// <summary>
    /// Used by OpenLinks tasks, when the URL of the page is set later during scraping.
    /// Keep internal.
    /// Initializes a new instance of the <see cref="PageCrawlJobBuilder"/> class.
    /// </summary>
    internal PageCrawlJobBuilder()
    {
    }

    /// <summary>
    /// Configures the page actions for the crawl job.
    /// </summary>
    /// <param name="options">The action to configure the page actions.</param>
    /// <returns>The updated <see cref="PageCrawlJobBuilder"/> instance.</returns>
    public PageCrawlJobBuilder ConfigurePageActions(Action<PageActionsBuilder> options)
    {
        var builder = new PageActionsBuilder();

        options(builder);

        PageActionsBuilder = builder;

        return this;
    }

    /// <summary>
    /// Adds a task to open a page and perform openLinks tasks.
    /// </summary>
    /// <param name="linksSelector">The CSS selector to select the element to openLinks.</param>
    /// <param name="tasks">The array of openLinks tasks to perform.</param>
    /// <param name="options">The action to configure the page actions for the openLinks tasks.</param>
    /// <returns>The updated <see cref="PageCrawlJobBuilder"/> instance.</returns>
    public PageCrawlJobBuilder OpenLinks(string linksSelector, Action<PageCrawlJobBuilder> jobOptions, Action<PageActionsBuilder>? options = null)
    {
        PageActionsBuilder? pageActionsBuilder = null;

        if (options is not null)
        {
            pageActionsBuilder = new PageActionsBuilder();

            options(pageActionsBuilder);
        }

        var builder = new PageCrawlJobBuilder();

        jobOptions(builder);

        Tasks.Add(new CrawlPageOpenLinksTask(linksSelector, builder, pageActionsBuilder?.Build()));

        return this;
    }

    /// <summary>
    /// Adds a task to paginate through.
    /// </summary>
    /// <param name="paginationSelector">The CSS selector to select the pagination element.</param>
    /// <returns>The updated <see cref="PageCrawlJobBuilder"/> instance.</returns>
    public PageCrawlJobBuilder AddPagination(string paginationSelector)
    {
        Tasks.Add(new CrawlPagePaginateTask(paginationSelector));

        return this;
    }

    /// <summary>
    /// Adds a task to scrape an object from the page using the specified selectors.
    /// </summary>
    /// <param name="selectors">The list of CSS selectors for the object to scrape.</param>
    /// <returns>The updated <see cref="PageCrawlJobBuilder"/> instance.</returns>
    public PageCrawlJobBuilder ExtractObject(List<CssSelector> selectors)
    {
        Tasks.Add(new CrawlPageExtractObjectTask(selectors));

        return this;
    }

    /// <summary>
    /// Adds a task to scrape a list of elements from the page using the specified list selector and selectors.
    /// </summary>
    /// <param name="containersSelector">The CSS selector for the containers of elements.</param>
    /// <param name="selectors">The list of CSS selectors for the elements to scrape.</param>
    /// <returns>The updated <see cref="PageCrawlJobBuilder"/> instance.</returns>
    public PageCrawlJobBuilder ExtractList(string containersSelector, List<CssSelector> selectors)
    {
        Tasks.Add(new CrawlPageExtractListTask(containersSelector, selectors));

        return this;
    }

    /// <summary>
    /// Adds a file crawl output to the page crawl job.
    /// </summary>
    /// <param name="filename">The filename of the output file.</param>
    /// <returns>The updated <see cref="PageCrawlJobBuilder"/> instance.</returns>
    public PageCrawlJobBuilder AddOutputToCsvFile(string filename = "results.csv", bool cleanOnStartup = true)
    {
        Outputs.Add(new CsvFileCrawlOutput(filename, cleanOnStartup));

        return this;
    }

    /// <summary>
    /// Adds a console crawl output to the page crawl job.
    /// </summary>
    /// <returns>The updated <see cref="PageCrawlJobBuilder"/> instance.</returns>
    public PageCrawlJobBuilder AddOutputToConsole()
    {
        Outputs.Add(new ConsoleCrawlOutput());

        return this;
    }

    /// <summary>
    /// Adds a custom crawl output to the page crawl job.
    /// </summary>
    /// <param name="outputAction">The custom crawl output action.</param>
    /// <returns>The updated <see cref="PageCrawlJobBuilder"/> instance.</returns>
    public PageCrawlJobBuilder AddOutput(params ICrawlOutput[] outputAction)
    {
        foreach (var output in outputAction)
        {
            if (!Outputs.Contains(output))
            {
                Outputs.Add(output);
            }
        }

        return this;
    }

    /// <summary>
    /// Used by openLinks action. Sets the URL later, keep internal.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="CocoCrawlerBuilderException"></exception>
    internal PageCrawlJobBuilder WithUrl(string url)
    {
        Url = url;
        return this;
    }

    /// <summary>
    /// Adds a task to the page crawl job.
    /// </summary>
    /// <param name="tasks">The task to add.</param>
    /// <returns>The updated <see cref="PageCrawlJobBuilder"/> instance.</returns>
    internal PageCrawlJobBuilder WithTasks(IPageCrawlTask[] tasks)
    {
        Tasks.AddRange(tasks);

        return this;
    }

    /// <summary>
    /// Replaces current outputs of the page. Keep Internal.
    /// </summary>
    /// <param name="tasks">The task to add.</param>
    /// <returns>The updated <see cref="PageCrawlJobBuilder"/> instance.</returns>
    internal PageCrawlJobBuilder ReplaceOutputs(ImmutableArray<ICrawlOutput> outputs)
    {
        Outputs = [];
        Outputs.AddRange(outputs);

        return this;
    }

    internal PageCrawlJob Build()
    {
        if (Tasks.Count == 0)
        {
            throw new CocoCrawlerBuilderException($"A Page requires a purpose, try calling .{nameof(OpenLinks)}() or .{nameof(ExtractObject)}() or {nameof(AddPagination)}().");
        }

        if (Url is null)
        {
            throw new CocoCrawlerBuilderException($"A Page requires a URL, try calling the constructor with a URL");
        }

        var pageActions = PageActionsBuilder?.Build();

        return new PageCrawlJob(Url, [.. Tasks], [.. Outputs], pageActions);
    }
}
