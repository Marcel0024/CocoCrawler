using CocoCrawler.Builders;
using CocoCrawler.Scheduler;
using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CocoCrawler.IntegrationTests.Engine;

public class UserAgentTests
{
    private readonly WireMockServer _wireMockServer = WireMockServer.Start();

    [Fact]
    public async Task UserAgent_Should_Be_Overwritten()
    {
        // Arange
        _wireMockServer.Given(Request.Create().WithUrl($"{_wireMockServer.Url}/useragent"))
            .RespondWith(Response.Create().WithSuccess());

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
