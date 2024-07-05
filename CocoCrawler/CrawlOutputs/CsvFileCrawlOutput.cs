using CocoCrawler.Outputs;
using Newtonsoft.Json.Linq;

namespace CocoCrawler.CrawlOutputs;

public class CsvFileCrawlOutput(string filePath, bool cleanOnStartup) : ICrawlOutput
{
    public bool CleanOnStartup { get; init; } = cleanOnStartup;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _initialized = false;

    public virtual async Task Initiaize(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (CleanOnStartup && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public virtual async Task WriteAsync(JObject jObject, CancellationToken cancellationToken)
    {
        var csv = string.Join(",", jObject.Properties().Select(p => p.Value));

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            // Add headers
            if (!File.Exists(filePath))
            {
                var headers = string.Join(",", jObject.Properties().Select(p => p.Name));
                await File.WriteAllTextAsync(filePath, headers + Environment.NewLine, cancellationToken);
            }

            await File.AppendAllTextAsync(filePath, csv + Environment.NewLine, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
