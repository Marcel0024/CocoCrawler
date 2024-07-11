# CocoCrawler 🥥

[![NuGet](https://img.shields.io/nuget/v/CocoCrawler?logo=nuget&logoColor=fff)](https://www.nuget.org/packages/CocoCrawler)
[![Build and Publish](https://github.com/Marcel0024/CocoCrawler/actions/workflows/main.yml/badge.svg)](https://github.com/Marcel0024/CocoCrawler/actions/workflows/main.yml)


`CocoCrawler` is an easy to use web crawler, scraper and parser in C#. By combing `PuppeteerSharp` and `AngleSharp` it brings the best of both sides, and merges them into an easy to use API.

It provides an simple API to get started

```csharp
var crawlerEngine = await new CrawlerEngineBuilder()
    .AddPage("https://old.reddit.com/r/csharp", pageOptions => pageOptions
        .ExtractList(containersSelector: "div.thing.link.self", [
            new("Title","a.title"),
            new("Upvotes", "div.score.unvoted"),
            new("Datetime", "time", "datetime"),
            new("Total Comments","a.comments"),
            new("Url","a.title", "href")
        ])
        .AddPagination("span.next-button > a")
        .ConfigurePageActions(options => // Only for showing the possibilities, not needed for running sample
        {
            options.ScrollToEnd();
            options.Wait(2000);
            // options.Click("span.next-button > a");
        })
        .AddOutputToConsole()
        .AddOutputToCsvFile("results.csv")
    )
    .ConfigureEngine(options =>
    {
        options.UseHeadlessMode(false);
        options.PersistVisitedUrls();
        options.WithLoggerFactory(loggerFactory);
        options.WithCookies([
            new("auth-cookie", "l;alqpekcoizmdfugnvkjgvsaaprufc", "thedomain.com")
        ]);
    })
    .BuildAsync(cancellationToken);

await crawlerEngine.RunAsync(cancellationToken);
```

This examples starts at page `https://old.reddit.com/r/csharp` scrapes all the posts, then continues to the next page and scrapes everything again, and on and on. And outputs everything scraped to the console and a csv file.

With this library it's easy to 

* Scrape Single Page Apps
* Scrape Listings
* Add pagination
* Alternative to list is open each post and scrape the page and continue with pagination
* Scrape multiple pages in parallel
* Add custom outputs
* Customize Everything

## Scraping pages

With each Page (a page a is a single URL job) added it's possible to add a Task. For each Page it's possible to:

### `.ExtractObject(...)`
```csharp
   var crawlerEngine = await new CrawlerEngineBuilder()
       .AddPage("https://github.com/", pageOptions => pageOptions
           .ExtractObject([
                new(Name: "Title", Selector: "div.title > a > span"),
                new(Name: "Description", Selector: "div.title > a > span"),
            ])
        .BuildAsync(cancellationToken);
```

Which scrapes the title and description of the page and outputs it. 

### `.ExtractList(...)`

```csharp
var crawlerEngine = await new CrawlerEngineBuilder()
    .AddPage("https://github.com/", pageOptions => pageOptions
        .ExtractList(containersSelector: "div > div.repos", [
            new(Name: "Title", Selector: "div.title > a > span"),
            new(Name: "Description", Selector: "div.title > a > span"),
        ]))
    .BuildAsync(cancellationToken);
```
ExtractList scrapes a list of objects. The `containersSelector` is the selector for the container that holds the objects. And all selectors after that are relative to the container.
Each object in the list is inidividually send to the output.


### `.OpenLinks(...)`

```csharp
var crawlerEngine = await new CrawlerEngineBuilder()
    .AddPage("https://github.com/", pageOptions => pageOptions
        .OpenLinks(linksSelector: "div.example-link-to-repose", subPage => subPage
            .ExtractObject([
                new("Title","div.sitetable.linklisting a.title"),
            ])))
    .BuildAsync(cancellationToken);
```

OpenLinks opens each link in the `linksSelector` and scrapes that page. It's usually combined with `.ExtractObject(...)` and `.AddPagination(...)`. `linksSelector` expects a list of a tags. It's also possible to chain multiple `.OpenLinks(...)`.


### `.AddPagination(...)`

```csharp
var crawlerEngine = await new CrawlerEngineBuilder()
    .AddPage("https://github.com/", pageOptions => pageOptions
        .ExtractList(containersSelector: "div > div.repos", [
            new(Name: "Title", Selector: "div.title > a > span"),
            new(Name: "Description", Selector: "div.title > a > span"),
        ]))
        .AddPagination("span.next-button > a")
    .BuildAsync(cancellationToken);
```

AddPagination adds pagination to the page. It expects a selector to the next page. It's usually the `Next` button.


## Multiple Pages

It's possible to add multiple pages to scrape with the same Tasks.

```csharp
   var crawlerEngine = await new CrawlerEngineBuilder()
       .AddPages(["https://old.reddit.com/r/csharp", "https://old.reddit.com/r/dotnet"], pageOptions => pageOptions
           .OpenLinks("div.thing.link.self a.bylink.comments", subPageOptions =>
           {
                subPageOptions.ExtractObject([
                       new("Title","div.sitetable.linklisting a.title"),
                       new("Url","div.sitetable.linklisting a.title", "href"),
                       new("Upvotes", "div.sitetable.linklisting div.score.unvoted"),
                       new("Top comment", "div.commentarea div.entry.unvoted div.md"),
               ]);
               subPageOptions.ConfigurePageActions(ops =>
                {
                    ops.ScrollToEnd();
                    ops.Wait(4000);
                });
           })
           .AddPagination("span.next-button > a")
        .BuildAsync(cancellationToken);

   await crawlerEngine.RunAsync(cancellationToken);
```
This example starts at `https://old.reddit.com/r/csharp` and `https://old.reddit.com/r/dotnet` and opens each post and scrapes the title, url, upvotes and top comment. It also scrolls to the end of the page and waits 4 seconds before scraping the page. And then it continues with the next pagination page.


## PageActions - A way to interact with the browser

Page Actions are a way to interact with the browser. It's possible to add page actions to each page. It's possible to click away popups, or scroll to bottom. The following actions are available:

```csharp
var crawlerEngine = await new CrawlerEngineBuilder()
    .AddPage("https://github.com/", pageOptions => pageOptions
        .ExtractList(containersSelector: "div > div.repos", [
            new(Name: "Title", Selector: "div.title > a > span"),
            new(Name: "Description", Selector: "div.title > a > span"),
        ]))
        .ConfigurePageActions(ops =>
        {
            ops.ScrollToEnd();
            ops.Click("button#load-more");
            ops.Wait(4000);
        });
    .BuildAsync(cancellationToken);
```

## Outputs

It's possible to add multiple outputs to the engine. The following outputs are available:

```csharp
var crawlerEngine = await new CrawlerEngineBuilder()
    .AddPage("https://github.com/", pageOptions => pageOptions
        .OpenLinks(linksSelector: "div.example-link-to-repose", subPage => subPage
            .ExtractObject([
                new("Title","div.sitetable.linklisting a.title"),
            ])))
        .AddOutputToConsole()
        .AddOutputToCsvFile("results.csv")    
    .BuildAsync(cancellationToken);
```

You can add your own output by implementing the `ICrawlOutput` interface.

```csharp
public interface ICrawlOutput
{
    Task Initiaize(CancellationToken cancellationToken);
    Task WriteAsync(JObject jObject, CancellationToken cancellationToken);
}
```

Initialize is called once before the engine starts. WriteAsync is called for each object that is scraped.

On Page level it's possible to add custom outputs

```csharp
var crawlerEngine = await new CrawlerEngineBuilder()
    .AddPage("", p => p.AddOutput(new MyCustomOutput()))
    .BuildAsync(cancellationToken);
```

## Configuring the Engine

### Cookies

It's possible to add cookies to all request

```csharp
var crawlerEngine = await new CrawlerEngineBuilder()
    .AddPage(...)
    .ConfigureEngine(options =>
    {
        options.WithCookies([
            new("auth-cookie", "l;alqpekcoizmdfugnvkjgvsaaprufc", "thedomain.com"),
            new("Cookie2", "def", "localhost")
        ]);
    })
    .BuildAsync(cancellationToken);
```

### Setting the User Agent

```csharp
var crawlerEngine = await new CrawlerEngineBuilder()
    .AddPage(...)
    .ConfigureEngine(options =>
    {
        options.WithUserAgent("linux browser - example user agent");
    })
    .BuildAsync(cancellationToken);
```
Default User Agent is from Chrome browser.

### Ignoring URLS

```csharp
var crawlerEngine = await new CrawlerEngineBuilder()
    .AddPage(...)
    .ConfigureEngine(options =>
    {
        options.WithIgnoreUrls(["https://example.com", "https://example2.com"]);
    })    
    .BuildAsync(cancellationToken);
```

### Stopping the engine

The engine stops when the 
* The total number of pages to crawl is reached.
* 2 minutes have passed since the last job was added

### Persisting visited pages

It's possible to persist visited pages to a file. Once persisted the engine will skip the pages next time.

```csharp
var crawlerEngine = await new CrawlerEngineBuilder()
    .AddPage(...)
    .ConfigureEngine(options =>
    {
        options.PersistVisitedUrls();
    })
    .BuildAsync(cancellationToken);
```

### Other notable options
The engine can be configured with the following options:

* `UseHeadlessMode(bool headless)`: If the browser should be headless or not
* `WithLoggerFactory(ILoggerFactory loggerFactory)`: The logger factory to use, to enable logging.
* `TotalPagesToCrawl(int total)`: The total number of pages to crawl
* `WithParallelismDegree(int parallelismDegree)` : The number of browser tabs it can open in parallel

## Extensibility

The library is designed to be extensible. It's possible to add custom `IParser`, `IScheduler`, `IVisitedUrlTracker` and `ICrawler` implementations.

using the engine builder it's possible to add custom implementations

```csharp
.ConfigureEngine(options =>
{
    options.WithCrawler(new MyCustomCrawler());
    options.WithScheduler(new MyCustomScheduler());
    options.WithParser(new MyCustomParser());
    options.WithVisitedUrlTracker(new MyCustomParser());
})
```

| Interfaces           | Description                                                                                                        |
| -------------------- | ------------------------------------------------------------------------------------------------------------------ |
| `IParser`            | IParser uses default AngleSharp. If you want to use something else then CSS selector, overwrite this.              |
| `IVisitedUrlTracker` | Default uses in memory tracker. It's possible to persist to a file. Those two options are available in the libary. |
| `IScheduler`         | Holds the current Jobs.                                                                                            |
