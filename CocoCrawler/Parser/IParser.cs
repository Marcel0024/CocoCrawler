using CocoCrawler.Job.PageTasks;
using Newtonsoft.Json.Linq;

namespace CocoCrawler.Parser;

public interface IParser
{
    Task Init(string html);
    string[] ParseForLinks(string linksSelector);
    JArray ExtractList(CrawlPageExtractListTask scrapeList);
    JObject ExtractObject(CrawlPageExtractObjectTask task);
}
