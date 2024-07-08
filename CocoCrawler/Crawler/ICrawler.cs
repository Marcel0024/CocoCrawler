using CocoCrawler.Job;
using CocoCrawler.Parser;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace CocoCrawler.Crawler;

public interface ICrawler
{
    void WithParser(IParser parser);
    void WithLoggerFactory(ILoggerFactory loggerFactory);
    Task<CrawlResult> Crawl(IPage browserTab, PageCrawlJob job);
}
