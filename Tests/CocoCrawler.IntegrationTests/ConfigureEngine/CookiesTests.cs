using CocoCrawler.Builders;
using CocoCrawler.Scheduler;
using FluentAssertions;
using WireMock.Server;

namespace CocoCrawler.IntegrationTests.Engine;

[Collection(nameof(BrowserCollection))]
public class CookiesTests
{
    private readonly WireMockServer _wireMockServer = WireMockServer.Start();

    [Fact]
    public async Task Cookies_Should_Be_Send_To_The_Client()
    {
        // Arange
        _wireMockServer.ReturnSuccessFor($"{_wireMockServer.Url}/cookies");

        var crawlerEngine = await new CrawlerEngineBuilder()
            .AddPage($"{_wireMockServer.Url}/cookies", options => options.ExtractObject([new("No exist", "div.test")]))
            .ConfigureEngine(ops =>
            {
                ops.WithScheduler(new InMemoryScheduler(totalSecondsTimeoutAfterJob: 2));
                ops.WithCookies([
                    new("Cookie1","abc", "localhost"),
                    new("Cookie2","def", "localhost")
                ]);
            })
            .BuildAsync();

        // Act
        await crawlerEngine.RunAsync();

        // Assert
        var cookiesSent = _wireMockServer.LogEntries.First()?.RequestMessage.Cookies;

        cookiesSent.Should().BeEquivalentTo(new Dictionary<string, string>()
        {
            { "Cookie1", "abc" },
            { "Cookie2", "def" }
        });
    }
}
