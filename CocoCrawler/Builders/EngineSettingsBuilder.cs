﻿
using CocoCrawler.Crawler;
using CocoCrawler.CrawlJob;
using CocoCrawler.Exceptions;
using CocoCrawler.Parser;
using CocoCrawler.Scheduler;
using CocoCrawler.VisitedUrlTracker;
using Microsoft.Extensions.Logging;

namespace CocoCrawler.Builders;

public class EngineSettingsBuilder
{
    private bool Headless { get; set; } = true;
    private string[] IgnoreUrls { get; set; } = [];
    private int ParallelismDegree { get; set; } = 5;
    private int MaxPagesToCrawl { get; set; } = 100;
    private bool ParallelismDisabled { get; set; } = false;
    private string? UserAgent { get; set; } = null;
    private ILoggerFactory? LoggerFactory { get; set; } = null;
    private IScheduler Scheduler { get; set; } = new InMemoryScheduler();
    private IParser Parser { get; set; } = new AngleSharpParser();
    private ICrawler Crawler { get; set; } = new PuppeteerCrawler();
    private Cookie[]? Cookies { get; set; } = null;
    private IVisitedUrlTracker? VisitedUrlTracker { get; set; } = null;

    /// <summary>
    /// Sets the headless mode for the browser.
    /// </summary>
    /// <param name="headless">A value indicating whether the browser should run in headless mode.</param>
    /// <returns>The <see cref="EngineSettingsBuilder"/> instance.</returns>
    public EngineSettingsBuilder UseHeadlessMode(bool headless)
    {
        Headless = headless;

        return this;
    }

    /// <summary>
    /// Sets the URLs to ignore during crawling.
    /// </summary>
    /// <param name="ignoreUrls">The URLs to ignore.</param>
    /// <returns>The <see cref="EngineSettingsBuilder"/> instance.</returns>
    public EngineSettingsBuilder WithIgnoreUrls(params string[] ignoreUrls)
    {
        IgnoreUrls = ignoreUrls;

        return this;
    }

    /// <summary>
    /// Sets the degree of parallelism for crawling.
    /// Runs in a single browser but a Tab for each crawl job.
    /// This specifies how many Tabs in Parallel.
    /// </summary>
    /// <param name="parallelismDegree">The degree of parallelism.</param>
    /// <returns>The <see cref="EngineSettingsBuilder"/> instance.</returns>
    public EngineSettingsBuilder WithParallelismDegree(int parallelismDegree)
    {
        ParallelismDegree = parallelismDegree;

        return this;
    }

    /// <summary>
    /// Sets the total pages to crawl.
    /// </summary>
    /// <param name="total">The total pages to crawl.</param>
    /// <returns>The <see cref="EngineSettingsBuilder"/> instance.</returns>
    public EngineSettingsBuilder TotalPagesToCrawl(int total)
    {
        MaxPagesToCrawl = total;

        return this;
    }

    /// <summary>
    ///  Run all jobs in a single Tab in a Single Browser.
    ///  This ignore the ParallelismDegree.
    /// </summary>
    /// <returns>The <see cref="EngineSettingsBuilder"/> instance.</returns>
    public EngineSettingsBuilder DisableParallelism()
    {
        ParallelismDisabled = true;

        return this;
    }

    /// <summary>
    /// Sets the scheduler to be used by the crawler engine.
    /// </summary>
    /// <param name="scheduler">The scheduler implementation.</param>
    /// <returns>The CrawlerEngineBuilder instance.</returns>
    public EngineSettingsBuilder WithScheduler(IScheduler scheduler)
    {
        Scheduler = scheduler;

        return this;
    }

    /// <summary>
    /// Sets the parser to be used by the crawler.
    /// </summary>
    /// <param name="parser">The crawler implementation.</param>
    /// <returns>The CrawlerEngineBuilder instance.</returns>
    public EngineSettingsBuilder WithParser(IParser parser)
    {
        Parser = parser;

        return this;
    }

    /// <summary>
    /// Sets the crawler to be used by the crawler engine.
    /// </summary>
    /// <param name="crawler">The crawler implementation.</param>
    /// <returns>The CrawlerEngineBuilder instance.</returns>
    public EngineSettingsBuilder WithCrawler(ICrawler crawler)
    {
        Crawler = crawler;

        return this;
    }

    /// <summary>
    /// Sets the user agent for the browser.
    /// </summary>
    /// <param name="userAgent">The user agent string.</param>
    /// <returns>The <see cref="EngineSettingsBuilder"/> instance.</returns>
    public EngineSettingsBuilder WithUserAgent(string userAgent)
    {
        UserAgent = userAgent;

        return this;
    }

    /// <summary>
    /// Sets the logger factory for the engine.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <returns>The <see cref="EngineSettingsBuilder"/> instance.</returns>
    public EngineSettingsBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        LoggerFactory = loggerFactory;

        return this;
    }

    /// <summary>
    /// Sets the cookies to be used by the crawler engine.
    /// </summary>
    /// <param name="cookies">The cookies to be used.</param>
    /// <returns>The <see cref="EngineSettingsBuilder"/> instance.</returns>
    public EngineSettingsBuilder WithCookies(params Cookie[] cookies)
    {
        Cookies = cookies;

        return this;
    }

    /// <summary>
    /// Sets the visited URL tracker to be used by the crawler engine.
    /// </summary>
    /// <param name="visitedUrlTracker">The visited URL tracker implementation.</param>
    /// <returns>The <see cref="EngineSettingsBuilder"/> instance.</returns>
    public EngineSettingsBuilder WithVisitedUrlTracker(IVisitedUrlTracker visitedUrlTracker)
    {
        VisitedUrlTracker = visitedUrlTracker;

        return this;
    }

    /// <summary>
    /// Sets the visited URL tracker to be used by the crawler engine with a file-based implementation.
    /// </summary>
    /// <param name="fileName">The file name to store the visited URLs.</param>
    /// <param name="cleanOnStart">A value indicating whether to clean the visited URLs file on engine start.</param>
    /// <returns>The <see cref="EngineSettingsBuilder"/> instance.</returns>
    public EngineSettingsBuilder PersistVisitedUrls(string fileName = "visited-links.txt", bool cleanOnStart = false)
    {
        VisitedUrlTracker = new FileVisitedUrlTracker(fileName, cleanOnStart);

        return this;
    }

    internal EngineSettings Build()
    {
        if (ParallelismDegree < 1)
        {
            throw new CocoCrawlerBuilderException("The parallelism degree must be greater than or equal to 1.");
        }

        if (MaxPagesToCrawl < 1)
        {
            throw new CocoCrawlerBuilderException("The total pages to crawl must be greater than or equal to 1.");
        }

        if (LoggerFactory is not null)
        {
            Crawler.WithLoggerFactory(LoggerFactory);
        }

        Crawler.WithParser(Parser);

        return new EngineSettings(Headless,
            ParallelismDisabled,
            IgnoreUrls,
            ParallelismDegree,
            MaxPagesToCrawl,
            UserAgent,
            Crawler,
            Scheduler,
            LoggerFactory,
            Cookies is null ? [] : [.. Cookies],
            VisitedUrlTracker ?? new InMemoryVisitedUrlTracker());
    }
}
