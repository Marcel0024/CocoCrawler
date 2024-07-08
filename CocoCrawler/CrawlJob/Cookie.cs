namespace CocoCrawler.CrawlJob;

public record Cookie(string Name, string Value, string Domain, string Path = "/");
