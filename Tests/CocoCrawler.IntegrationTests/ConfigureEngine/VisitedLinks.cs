using CocoCrawler.Builders;
using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CocoCrawler.IntegrationTests.ConfigureEngine;

[Collection(nameof(BrowserCollection))]
public class VisitedLinks
{
    private readonly WireMockServer _wireMockServer = WireMockServer.Start();

    [Fact]
    public async Task VisitedLinks_ShouldNotBePersisted_WithDefaultSettings()
    {
        // Arange
        SetUrls(_wireMockServer);

        var crawlerEngine = await new CrawlerEngineBuilder()
            .AddPage($"{_wireMockServer.Url}/main-page-0", options => options
                .ExtractObject([
                    new("Title", "div.content div.title"),
                ])
                .AddPagination("div.pagination > a"))
            .ConfigureEngine(ops => ops.TotalPagesToCrawl(30))
            .BuildAsync();

        // Act
        await crawlerEngine.RunAsync();
        await crawlerEngine.RunAsync();

        // Assert
        var visitedPages = _wireMockServer.LogEntries.Where(x => x.RequestMessage.AbsoluteUrl.Contains("main-page"));
        visitedPages.Should().HaveCount(60);
    }

    [Fact]
    public async Task VisitedLinks_ShouldBePersisted_WithPersistVisitedUrls()
    {
        // Arange
        SetUrls(_wireMockServer);

        var crawlerEngine = await new CrawlerEngineBuilder()
            .AddPage($"{_wireMockServer.Url}/main-page-0", options => options
                .ExtractObject([
                    new("Title", "div.content div.title"),
                ])
                .AddPagination("div.pagination > a"))
            .ConfigureEngine(ops => ops.TotalPagesToCrawl(30).PersistVisitedUrls())
            .BuildAsync();

        // Act
        await crawlerEngine.RunAsync();
        await crawlerEngine.RunAsync();
        await crawlerEngine.RunAsync();
        await crawlerEngine.RunAsync();

        // Assert
        var visitedPages = _wireMockServer.LogEntries.Where(x => x.RequestMessage.AbsoluteUrl.Contains("main-page"));
        visitedPages.Should().HaveCount(30);

        File.Delete("visited-links.txt");
    }

    private static void SetUrls(WireMockServer wireMockServer)
    {
        foreach (var index in Enumerable.Range(0, 31))
        {
            wireMockServer.Given(Request.Create().WithUrl($"{wireMockServer.Url}/main-page-{index}"))
               .RespondWith(Response.Create()
               .WithHeader("Content-Type", "text/xml; charset=utf-8")
               .WithBody(GetPage($"{wireMockServer.Url}/main-page-{index + 1}")));
        }
    }

    private static string GetPage(string nextUrl)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<body>
    <div class=""content"">
        <div class=""title"">Title Two</div>
        <span class=""details"">Detail</span>
    </div>
    <div class=""pagination"">
        <a href=""{nextUrl}"">Next</a>
    </dvi>
</body>
</html>
";
    }
}
