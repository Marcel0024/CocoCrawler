using CocoCrawler.Crawler;
using CocoCrawler.CrawlJob;
using CocoCrawler.Scheduler;
using CocoCrawler.VisitedUrlTracker;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace CocoCrawler;

public record EngineSettings(
    bool IsHeadless, 
    bool DisableParallelism,
    string[] IgnoreUrls, 
    int ParallelismDegree,
    int MaxPagesToCrawl,
    string? UserAgent,
    ICrawler Crawler,
    IScheduler Scheduler,
    ILoggerFactory? LoggerFactory,
    ImmutableArray<Cookie> Cookies,
    IVisitedUrlTracker VisitedUrlTracker);