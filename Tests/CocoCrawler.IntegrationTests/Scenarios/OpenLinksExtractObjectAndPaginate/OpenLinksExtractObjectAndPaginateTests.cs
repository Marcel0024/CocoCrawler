using CocoCrawler.Builders;
using CocoCrawler.Scheduler;
using FluentAssertions;
using WireMock.Server;

namespace CocoCrawler.IntegrationTests.Scenarios.OpenLinksExtractObjectAndPaginate;

[Collection(nameof(BrowserCollection))]
public class OpenLinksExtractObjectAndPaginateTests
{
    private readonly WireMockServer _wireMockServer = WireMockServer.Start(port: 9010);

    [Fact]
    public async Task OpenLinksExtractObjAndPaginate_ShouldHaveDetailsInFile_OnHappyFlow()
    {
        // Arange
        foreach (var index in Enumerable.Range(1, 10))
        {
            _wireMockServer.ReturnSuccessWithPage($"{_wireMockServer.Url}/main-page-{index}", GetListingHtmlPages(index));
        }

        foreach (var index in Enumerable.Range(1, 10 * 3)) // 3 listing per page
        {
            _wireMockServer.ReturnSuccessWithPage($"{_wireMockServer.Url}/content-page-{index}", GetContentPage(index));
        }

        var outputFile = Path.Combine("Scenarios", "OpenLinksExtractObjectAndPaginate", "Results", "resultstest1.csv");

        var crawlerEngine = await new CrawlerEngineBuilder()
            .AddPage($"{_wireMockServer.Url}/main-page-1", options => options
                .OpenLinks("div.content.test a.link", newPage => newPage
                    .ExtractObject([
                        new("Title", "div.content.test div.title"),
                        new("Description", "div.content.test div.description"),
                        new("Amount", "span.amount"),
                        new("Link", "a", "href")
                    ]))
                .AddPagination("div.pagination a:nth-last-child(1)")
                .AddOutputToCsvFile(outputFile))
            .ConfigureEngine(e => e
                .WithScheduler(new InMemoryScheduler(totalSecondsTimeoutAfterJob: 2))
                .WithIgnoreUrls(["http://localhost:9010/content-page-6"])
                .DisableParallelism())
            .BuildAsync();

        // Act
        await crawlerEngine.RunAsync();

        // Assert
        var outputContents = File.ReadAllText(outputFile);

        var expect = @"Url,Title,Description,Amount,Link
http://localhost:9010/content-page-1,Title 1,Description1,Amount 10,link1
http://localhost:9010/content-page-2,Title 2,Description2,Amount 20,link2
http://localhost:9010/content-page-3,Title 3,Description3,Amount 30,link3
http://localhost:9010/content-page-4,Title 4,Description4,Amount 40,link4
http://localhost:9010/content-page-5,Title 5,Description5,Amount 50,link5
http://localhost:9010/content-page-7,Title 7,Description7,Amount 70,link7
http://localhost:9010/content-page-8,Title 8,Description8,Amount 80,link8
http://localhost:9010/content-page-9,Title 9,Description9,Amount 90,link9
http://localhost:9010/content-page-10,Title 10,Description10,Amount 100,link10
http://localhost:9010/content-page-11,Title 11,Description11,Amount 110,link11
http://localhost:9010/content-page-12,Title 12,Description12,Amount 120,link12
http://localhost:9010/content-page-13,Title 13,Description13,Amount 130,link13
http://localhost:9010/content-page-14,Title 14,Description14,Amount 140,link14
http://localhost:9010/content-page-15,Title 15,Description15,Amount 150,link15
http://localhost:9010/content-page-16,Title 16,Description16,Amount 160,link16
http://localhost:9010/content-page-17,Title 17,Description17,Amount 170,link17
http://localhost:9010/content-page-18,Title 18,Description18,Amount 180,link18
http://localhost:9010/content-page-19,Title 19,Description19,Amount 190,link19
http://localhost:9010/content-page-20,Title 20,Description20,Amount 200,link20
http://localhost:9010/content-page-21,Title 21,Description21,Amount 210,link21
http://localhost:9010/content-page-22,Title 22,Description22,Amount 220,link22
http://localhost:9010/content-page-23,Title 23,Description23,Amount 230,link23
http://localhost:9010/content-page-24,Title 24,Description24,Amount 240,link24
http://localhost:9010/content-page-25,Title 25,Description25,Amount 250,link25
http://localhost:9010/content-page-26,Title 26,Description26,Amount 260,link26
http://localhost:9010/content-page-27,Title 27,Description27,Amount 270,link27
http://localhost:9010/content-page-28,Title 28,Description28,Amount 280,link28
http://localhost:9010/content-page-29,Title 29,Description29,Amount 290,link29
http://localhost:9010/content-page-30,Title 30,Description30,Amount 300,link30
";

        outputContents.Should().BeEquivalentTo(expect);

        File.Delete(outputFile);
    }

    private static string GetContentPage(int index)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title>Test A</title>
</head>
<body>
    <div class=""content""></div>
    <div class=""content test"">
       <div class=""title"">Title {index}</div>
       <div class=""description"">Description{index}</div>
       <span class=""amount"">Amount {index * 10}</span>
       <a class=""link"" href=""link{index}"">Link</a>
    </div>
</body>
</html>
";
    }

    private static string GetListingHtmlPages(int index)
    {
        int start = (index - 1) * 3;

        return $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title>Test A</title>
</head>
<body>
    <div class=""content""></div>
    <div class=""content test"">
        <div class=""listing {index}"">
            <div class=""title"">Title {index}</div>
            <a class=""link"" href=""http://localhost:9010/content-page-{start + 1}"" >Link</a>
        </div>
        <div class=""listing two"">
            <div class=""title"">Title Two</div>
            <a class=""link"" href=""http://localhost:9010/content-page-{start + 2}"">Link</a>
        </div>
        <div class=""listing three"">
            <div class=""title"">Title Three</div>
            <a class=""link"" href=""http://localhost:9010/content-page-{start + 3}"">Link</a>
        </div>
    </div>
    <div class=""pagination"">
      {(index == 10 ? "" : $"<a href=\"http://localhost:9010/main-page-{index + 1}\"")}>Next</a>""
    </div>
</body>
</html>
";
    }
}