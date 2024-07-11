using CocoCrawler.Builders;
using CocoCrawler.Scheduler;
using FluentAssertions;
using WireMock.Server;

namespace CocoCrawler.IntegrationTests.Engine;

[Collection(nameof(BrowserCollection))]
public class UserAgentTests
{
    private readonly WireMockServer _wireMockServer = WireMockServer.Start();

    [Fact]
    public async Task UserAgent_Should_Be_Overwritten()
    {
        // Arange
        _wireMockServer.ReturnSuccessFor($"{_wireMockServer.Url}/useragent");

        var crawlerEngine = await new CrawlerEngineBuilder()
            .AddPage($"{_wireMockServer.Url}/useragent", options => options.ExtractObject([new("No exist", "div.test")]))
            .ConfigureEngine(ops => ops.WithUserAgent("mock user agent aka not chrome").WithScheduler(new InMemoryScheduler(totalSecondsTimeoutAfterJob: 2)))
            .BuildAsync();

        // Act
        await crawlerEngine.RunAsync();

        // Assert
        var userAgentUsed = _wireMockServer.LogEntries.First()?.RequestMessage.Headers?["User-Agent"];

        userAgentUsed.Should().BeEquivalentTo("mock user agent aka not chrome");
    }
}
