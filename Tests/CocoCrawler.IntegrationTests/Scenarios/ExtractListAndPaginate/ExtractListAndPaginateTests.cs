using CocoCrawler.Builders;
using CocoCrawler.Scheduler;
using FluentAssertions;
using WireMock.Server;

namespace CocoCrawler.IntegrationTests.Scenarios.ExtractListAndPaginate;

[Collection(nameof(BrowserCollection))]
public class ExtractObjectAndPaginateTests
{
    private readonly WireMockServer _wireMockServer = WireMockServer.Start(port: 9090);

    [Fact]
    public async Task ExtractListAndPaginate_ShouldHaveDetailsInFile_OnHappyFlow()
    {
        // Arange
        _wireMockServer.ReturnSuccessWithBodyFromFile("http://localhost:9090/main-page", "Scenarios\\ExtractListAndPaginate\\Responses\\main-page.html");
        _wireMockServer.ReturnSuccessWithBodyFromFile("http://localhost:9090/page-2", "Scenarios\\ExtractListAndPaginate\\Responses\\page-2.html");

        var crawlerEngine = await new CrawlerEngineBuilder()
            .AddPage("http://localhost:9090/main-page", options => options
                .ExtractList("div.content.test > div.listing", [
                    new("Title", "div.title"),
                    new("Description", "div.description"),
                    new("Amount", "span.amount"),
                    new("Link", "a", "href")
                ])
                .AddPagination("div.pagination a:nth-last-child(1)")
                .AddOutputToCsvFile("ExtractListAndPaginate\\Results\\resultstest1.csv"))
            .ConfigureEngine(ops => ops.WithScheduler(new InMemoryScheduler(2)))
            .BuildAsync();

        // Act
        await crawlerEngine.RunAsync();

        // Assert
        var outputContents = File.ReadAllText("ExtractListAndPaginate\\Results\\resultstest1.csv");

        var expect = @"Title,Description,Amount,Link
Title One,Description1,10,/linkone
Title Two,Description2,20,/linktwo
Title Three,Description3,30,/linkthree
Title Four,Description4,40,/linkfour
Title Five,Description5,5,/linkfive
Title Six,Description6,6,/linksix
Title Seven,Description7,7,/linkseven
Title Eight,Description8,8,/linkeight
";

        outputContents.Should().BeEquivalentTo(expect);
    }
}