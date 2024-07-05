using CocoCrawler.Job;
using Newtonsoft.Json.Linq;

namespace CocoCrawler.Crawler;

public record CrawlResult(PageCrawlJob[] NewJobs, JArray ScrapedData);