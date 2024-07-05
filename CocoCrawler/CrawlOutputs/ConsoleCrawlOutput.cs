using CocoCrawler.Outputs;
using Newtonsoft.Json.Linq;

namespace CocoCrawler.CrawlOutputs;

public class ConsoleCrawlOutput : ICrawlOutput
{
    public Task Initiaize(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task WriteAsync(JObject obj, CancellationToken _)
    {
        Console.WriteLine(obj);
        return Task.CompletedTask;
    }
}
