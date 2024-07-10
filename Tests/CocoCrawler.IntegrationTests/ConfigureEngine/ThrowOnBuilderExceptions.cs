using CocoCrawler.Builders;
using CocoCrawler.Exceptions;
using FluentAssertions;
using WireMock.Server;

namespace CocoCrawler.IntegrationTests.Engine;

[Collection(nameof(BrowserCollection))]
public class ThrowOnBuilderExceptions
{
    private readonly WireMockServer _wireMockServer = WireMockServer.Start();

    [Fact]
    public async Task ShouldThrow_BuilderEx_WhenNoPagesAdded()
    {
        // Arange
        var crawlerEngine = new CrawlerEngineBuilder();

        // Act
        async Task<CrawlerEngine> act() => await crawlerEngine.BuildAsync();

        // Assert
        var ex = await Assert.ThrowsAsync<CocoCrawlerBuilderException>((Func<Task<CrawlerEngine>>)act);

        ex.Message.Should().Be("At least one Page is required to build the engine. Try calling .AddPage() to add pages.");
    }

    [Fact]
    public void ShouldThrow_BuilderEx_WhenNotValidUri()
    {
        // Arange
        var crawlerEngine = new CrawlerEngineBuilder();

        // Act
        CrawlerEngineBuilder act() => crawlerEngine.AddPage("notvalid", _ => { });

        // Assert
        var ex = Assert.Throws<CocoCrawlerBuilderException>(act);

        ex.Message.Should().Be("notvalid is not a valid URI.");
    }

    [Fact]
    public async Task ShouldThrow_BuilderEx_WhenPagesDontHaveTask()
    {
        // Arange
        var crawlerEngine = new CrawlerEngineBuilder()
            .AddPage("https://localhost:5000", _ => { });

        // Act
        async Task<CrawlerEngine> act() => await crawlerEngine.BuildAsync();

        // Assert
        var ex = await Assert.ThrowsAsync<CocoCrawlerBuilderException>((Func<Task<CrawlerEngine>>)act);

        ex.Message.Should().Be("A Page requires a purpose, try calling .OpenLinks() or .ExtractObject() or AddPagination().");
    }

    [Fact]
    public async Task ShouldThrow_BuilderEx_InvalidParallelismDegree()
    {
        // Arange
        var crawlerEngine = new CrawlerEngineBuilder()
            .AddPage("https://localhost:5000", o => o.ExtractObject([new("","")]))
            .ConfigureEngine(x => x.WithParallelismDegree(-1));

        // Act
        async Task<CrawlerEngine> act() => await crawlerEngine.BuildAsync();

        // Assert
        var ex = await Assert.ThrowsAsync<CocoCrawlerBuilderException>((Func<Task<CrawlerEngine>>)act);

        ex.Message.Should().Be("The parallelism degree must be greater than or equal to 1.");
    }

    [Fact]
    public async Task ShouldThrow_BuilderEx_WhenInvalidTotalPagesToCrawl()
    {
        // Arange
        var crawlerEngine = new CrawlerEngineBuilder()
            .AddPage("https://localhost:5000", o => o.ExtractObject([new("", "")]))
            .ConfigureEngine(x => x.TotalPagesToCrawl(-1));

        // Act
        async Task<CrawlerEngine> act() => await crawlerEngine.BuildAsync();

        // Assert
        var ex = await Assert.ThrowsAsync<CocoCrawlerBuilderException>((Func<Task<CrawlerEngine>>)act);

        ex.Message.Should().Be("The total pages to crawl must be greater than or equal to 1.");
    }
}
