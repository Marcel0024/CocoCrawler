using AngleSharp.Dom;
using CocoCrawler.Job.PageTasks;
using Newtonsoft.Json.Linq;

namespace CocoCrawler.Parser;

public class AngleSharpParser : IParser
{
    public virtual string[] GetUrlsFromSelector(IDocument doc, string selector)
    {
        return doc.QuerySelectorAll(selector)
            .Select(link => link.GetAttribute("href"))
            .Where(link => link is not null)
            .Select(link => link!)
            .ToArray();
    }

    public virtual JObject ExtractObject(IDocument doc, CrawlPageExtractObjectTask task)
    {
        return ParseObject(doc.DocumentElement!, task.Selectors);
    }

    public virtual JArray ExtractList(IDocument doc, CrawlPageExtractListTask scrapeList)
    {
        var jArray = new JArray();

        var containers = doc.QuerySelectorAll(scrapeList.ContentContainersSelector);

        if (containers is null || containers.Length == 0)
        {
            return jArray;
        }

        foreach (var container in containers)
        {
            jArray.Add(ParseObject(container, scrapeList.Selectors));
        }

        return jArray;
    }

    protected virtual JObject ParseObject(IElement node, IEnumerable<CssSelector> cssSelectors)
    {
        var jObject = new JObject();

        foreach (var selector in cssSelectors)
        {
            jObject[selector.Name] = GetSelectorValue(node, selector.Selector, selector.Attribute);
        }

        return jObject;
    }

    protected virtual string? GetSelectorValue(IElement element, string selector, string? attribute)
    {
        var value = element.QuerySelector(selector);

        return attribute is not null
             ? value?.GetAttribute(attribute)
             : value?.Text().Trim();
    }
}

