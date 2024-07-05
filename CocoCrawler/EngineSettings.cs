using CocoCrawler.Crawler;
using CocoCrawler.Parser;
using CocoCrawler.Scheduler;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CocoCrawler;

public record EngineSettings(
    bool IsHeadless, 
    bool DisableParallelism,
    string[] IgnoreUrls, 
    int ParallelismDegree,
    int MaxPagesToCrawl,
    string? UserAgent,
    IParser Parser,
    ICrawler Crawler,
    IScheduler Scheduler,
    ILoggerFactory? LoggerFactory,
    Cookie[] Cookies,
    HistoryTracker HistoryTracker);