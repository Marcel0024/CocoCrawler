using CocoCrawler.Builders;
using CocoCrawler.Scheduler;
using FluentAssertions;
using WireMock.Server;

namespace CocoCrawler.IntegrationTests.Outputs;

[Collection(nameof(BrowserCollection))]
public class CsvOutputTests
{
    private readonly WireMockServer _wireMockServer = WireMockServer.Start();

    [Fact]
    public async Task CsvOutput_ShouldCreateFile_WhenCleanOnStartup()
    {
        // Arange
        _wireMockServer.ReturnSuccessWithPage($"{_wireMockServer.Url}/main-page", GetPage());

        var outputPath = Path.Combine("Outputs", "Really", "Deep", "Path", "resultstest1.csv");

        var crawlerEngine = await new CrawlerEngineBuilder()
            .AddPage($"{_wireMockServer.Url}/main-page", options => options
                .ExtractObject([
                    new("Title", "div.content div.title"),
                    new("Details", "div.content span.details"),
                ])
                .AddOutputToCsvFile(outputPath, cleanOnStartup: true))
            .ConfigureEngine(ops => ops.WithScheduler(new InMemoryScheduler(totalSecondsTimeoutAfterJob: 2)))
            .BuildAsync();

        // Act
        await crawlerEngine.RunAsync();
        var results1 = File.ReadAllText(outputPath);

        await crawlerEngine.RunAsync();
        var results2 = File.ReadAllText(outputPath);

        // Assert
        var exptected = @$"Url,Title,Details
{_wireMockServer.Url}/main-page,Title Two,Detail
";

        results1.Should().Be(exptected);
        results2.Should().Be(exptected);
    }

    [Fact]
    public async Task CsvOutput_ShouldNotCreateFile_OnCleanOnStartupFalse()
    {
        // Arange
        _wireMockServer.ReturnSuccessWithPage($"{_wireMockServer.Url}/main-page", GetPage());

        var outputPath = Path.Combine("Outputs", "Really", "Deep", "Path", $"results-dont-clean-{Random.Shared.Next(0,100)}.csv");

        var crawlerEngine = await new CrawlerEngineBuilder()
            .AddPage($"{_wireMockServer.Url}/main-page", options => options
                .ExtractObject([
                    new("Title", "div.content div.title"),
                    new("Details", "div.content span.details"),
                ])
                .AddOutputToCsvFile(outputPath, cleanOnStartup: false))
            .ConfigureEngine(ops => ops.WithScheduler(new InMemoryScheduler(totalSecondsTimeoutAfterJob: 2)))
            .BuildAsync();

        // Act
        await crawlerEngine.RunAsync();
        var results1 = File.ReadAllText(outputPath);

        await crawlerEngine.RunAsync();
        var results2 = File.ReadAllText(outputPath);

        // Assert
        var exptected1 = @$"Url,Title,Details
{_wireMockServer.Url}/main-page,Title Two,Detail
";

        results1.Should().Be(exptected1);

        var exptected2 = @$"Url,Title,Details
{_wireMockServer.Url}/main-page,Title Two,Detail
{_wireMockServer.Url}/main-page,Title Two,Detail
";

        results2.Should().Be(exptected2);

        File.Delete(outputPath);
    }

    private static string GetPage()
    {
        return $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<body>
    <div class=""content"">
        <div class=""title"">Title Two</div>
        <span class=""details"">Detail</span>
    </div>
</body>
</html>
";
    }
}
