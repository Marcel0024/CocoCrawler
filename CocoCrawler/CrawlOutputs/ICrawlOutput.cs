using Newtonsoft.Json.Linq;

namespace CocoCrawler.Outputs;

public interface ICrawlOutput
{
    Task Initiaize(CancellationToken cancellationToken);
    Task WriteAsync(JObject jObject, CancellationToken cancellationToken);
}
