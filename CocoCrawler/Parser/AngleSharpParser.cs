using AngleSharp;
using AngleSharp.Dom;
using CocoCrawler.Job.PageTasks;
using Newtonsoft.Json.Linq;

namespace CocoCrawler.Parser;

public class AngleSharpParser : IParser
{
    private IDocument? _document;

    public virtual async Task Init(string html)
    {
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);

        _document = await context.OpenAsync(req => req.Content(html));
    }

    public virtual string[] ParseForLinks(string linksSelector, Func<IElement, string?>? linkProcessor = null)
    {
        linkProcessor ??= (element) => element.GetAttribute("href");

        return _document!.QuerySelectorAll(linksSelector)
            .Select(link => linkProcessor(link))
            .Where(link => link is not null)
            .Select(link => link!)
            .ToArray();
    }

    public virtual JObject ExtractObject(CrawlPageExtractObjectTask task)
    {
        return ParseObject(_document!.DocumentElement, task.Selectors);
    }

    public virtual JArray ExtractList(CrawlPageExtractListTask scrapeList)
    {
        var jArray = new JArray();

        var containers = _document!.QuerySelectorAll(scrapeList.ContentContainersSelector);

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

