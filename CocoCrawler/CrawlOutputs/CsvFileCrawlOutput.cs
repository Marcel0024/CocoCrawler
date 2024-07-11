using CocoCrawler.Outputs;
using Newtonsoft.Json.Linq;

namespace CocoCrawler.CrawlOutputs;

public class CsvFileCrawlOutput(string filePath, bool cleanOnStartup) : ICrawlOutput
{
    public bool CleanOnStartup { get; init; } = cleanOnStartup;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public virtual Task Initiaize(CancellationToken cancellationToken)
    {
        if (CleanOnStartup && File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        return Task.CompletedTask;
    }

    public virtual async Task WriteAsync(JObject jObject, CancellationToken cancellationToken)
    {
        var csv = string.Join(",", jObject.Properties().Select(p => p.Value));

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            // Add headers if new file
            if (!File.Exists(filePath))
            {
                var headers = string.Join(",", jObject.Properties().Select(p => p.Name));
                await File.AppendAllLinesAsync(filePath, [headers], cancellationToken);
            }

            await File.AppendAllLinesAsync(filePath, [csv], cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
