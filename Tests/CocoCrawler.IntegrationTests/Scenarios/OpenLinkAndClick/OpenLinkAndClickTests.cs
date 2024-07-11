using CocoCrawler.Builders;
using CocoCrawler.Scheduler;
using FluentAssertions;
using WireMock.Server;

namespace CocoCrawler.IntegrationTests.Scenarios.OpenLinkAndClick;

[Collection(nameof(BrowserCollection))]
public class OpenLinkAndClickTests
{
    private readonly WireMockServer _wireMockServer = WireMockServer.Start();

    [Fact]
    public async Task DocumentShould_Click_WhenCalled()
    {
        // Arrange
        _wireMockServer.ReturnSuccessWithPage($"{_wireMockServer.Url}/clickme", GeStartPage(_wireMockServer.Url!));
        _wireMockServer.ReturnSuccessWithPage($"{_wireMockServer.Url}/next-page", GetSecondPage());

        var crawlerEngine = await new CrawlerEngineBuilder()
            .AddPage($"{_wireMockServer.Url}/clickme", pageOptions => pageOptions
                .OpenLinks("div.content > a", subPageOptions =>
                {
                    subPageOptions.ConfigurePageActions(actions =>
                    {
                        actions.Click("button#clickme");
                    });
                    subPageOptions.ExtractObject([new("Was i clicked", "div.clicked-now-scraped")]);
                })
                .AddOutputToCsvFile("clicked-results.txt", cleanOnStartup: true)
            )
            .ConfigureEngine(options => options.WithScheduler(new InMemoryScheduler(totalSecondsTimeoutAfterJob: 2)))
            .BuildAsync();

        // Act
        await crawlerEngine.RunAsync();

        // Assert
        var fileOutputContents = File.ReadAllText("clicked-results.txt");

        var expectedContents = $@"Url,Was i clicked
{_wireMockServer.Url}/next-page,Yes i was!
";

        fileOutputContents.Should().Be(expectedContents);
    }

    private static string GeStartPage(string baseUrl)
    {
        return $@"
            <html>
                <body>
                    <div class=""content"">
                        <a href='{baseUrl}/next-page'>Click me</a>
                    </div>
                </body>
            </html>";
    }

    private static string GetSecondPage()
    {
        return @"
            <!DOCTYPE html>
            <html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
                <body>
                   <button id=""clickme""> ClickMe </button>
                   <script>
                        document.getElementById('clickme').addEventListener('click', function() {
                            var div = document.createElement('div');
                            div.className = 'clicked-now-scraped';
                            div.textContent = 'Yes i was!'; 
                            document.body.appendChild(div);
                        });
                   </script>
                </body>              
            </html>";
    }
}
