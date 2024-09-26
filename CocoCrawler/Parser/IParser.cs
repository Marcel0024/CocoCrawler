using AngleSharp.Dom;
using CocoCrawler.Job.PageTasks;
using Newtonsoft.Json.Linq;

namespace CocoCrawler.Parser;

public interface IParser
{
    Task Init(string html);
    string[] ParseForLinks(string linksSelector, Func<IElement, string?>? linkProcessor = null);
    JArray ExtractList(CrawlPageExtractListTask scrapeList);
    JObject ExtractObject(CrawlPageExtractObjectTask task);
}
