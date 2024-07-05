using AngleSharp.Dom;
using CocoCrawler.Job.PageTasks;
using Newtonsoft.Json.Linq;

namespace CocoCrawler.Parser;

public interface IParser
{
    string[] GetUrlsFromSelector(IDocument doc, string selector);
    JArray ExtractList(IDocument doc, CrawlPageExtractListTask scrapeList);
    JObject ExtractObject(IDocument doc, CrawlPageExtractObjectTask task);
}
