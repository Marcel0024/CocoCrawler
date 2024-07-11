using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CocoCrawler.IntegrationTests;

internal static class WireMockExtensions
{
    internal static void ReturnSuccessFor(this WireMockServer wireMockServer, string url)
    {
        wireMockServer
            .Given(Request.Create()
                .UsingGet()
                .WithUrl(url))
            .RespondWith(Response.Create()
                .WithSuccess());
    }

    internal static void ReturnSuccessWithBodyFromFile(this WireMockServer wireMockServer, string url, string filePath)
    {
        wireMockServer
            .Given(Request.Create()
                .UsingGet()
                .WithUrl(url))
            .RespondWith(Response.Create()
                .WithHeader("Content-Type", "text/xml; charset=utf-8")
                .WithSuccess()
                .WithBodyFromFile(filePath));
    }

    internal static void ReturnSuccessWithPage(this WireMockServer wireMockServer, string url, string page)
    {
        wireMockServer
            .Given(Request.Create()
                .UsingGet()
                .WithUrl(url))
            .RespondWith(Response.Create()
                .WithHeader("Content-Type", "text/xml; charset=utf-8")
                .WithSuccess()
                .WithBody(page));
    }
}
