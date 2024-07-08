# CocoCrawler

[![NuGet](https://img.shields.io/nuget/v/CocoCrawler?logo=nuget&logoColor=fff)](https://www.nuget.org/packages/CocoCrawler)
[![Build and Publish](https://github.com/Marcel0024/CocoCrawler/actions/workflows/main.yml/badge.svg)](https://github.com/Marcel0024/CocoCrawler/actions/workflows/main.yml)

## Overview

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
        .AddPagination("span.next-button > a.not-exist", newPage => newPage.ScrollToEnd())
        .AddOutputToConsole()
        .AddOutputToCsvFile("results.csv")
    )
    .ConfigureEngine(options =>
    {
        options.UseHeadlessMode(false);
        options.WithLoggerFactory(loggerFactory);
    })
    .BuildAsync(cancellationToken);

await crawlerEngine.RunAsync(cancellationToken);
```

This examples starts at page `https://old.reddit.com/r/csharp` scrapes all the posts, then continues to the next page and scrapes everything again, and on and on.

With this library it's easy to 

* Scrape Single Page Apps
* Scrape Listings
* Add pagination
* Alternative to list is open each post and scrape the page and continue with pagination
* Scrape multiple pages in parallel
* Add custom outputs
* Customize Everything

## Scraping pages

With each Page (a page a is a single URL job) added it's possible to add a Task. For each Page it's possible to call:

* `.ExtractObject(...)`
* `.ExtractList(...)`
* `.OpenLinks(...)`
* `.AddPagination(...)`

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
           .AddOutputToConsole()
           .AddOutputToCsvFile("results.csv"))
        .BuildAsync(cancellationToken);

   await crawlerEngine.RunAsync(cancellationToken);
```

This example starts at `https://old.reddit.com/r/csharp` and `https://old.reddit.com/r/dotnet` and opens each post and scrapes the title, url, upvotes and top comment. It also scrolls to the end of the page and waits 4 seconds before scraping the page. And then it continues with the next pagination page.



## Configuring the Engine

The engine can be configured with the following options:

* `UseHeadlessMode(bool headless)`: If the browser should be headless or not
* `WithLoggerFactory(ILoggerFactory loggerFactory)`: The logger factory to use
* `WithUserAgent(string userAgent)`: The user agent to use
* `WithCookies(params Cookie[] cookies)`: The cookies to use
* `TotalPagesToCrawl(int total)`: The total number of pages to crawl
* `WithParallelismDegree(int parallelismDegree)` : The number of pages to crawl in parallel

## Cookies

It's possible to add cookies to all request

```csharp
.ConfigureEngine(options =>
{
    options.WithCookies([
        new("auth-cookie", "l;alqpekcoizmdfugnvkjgvsaaprufc", "thedomain.com"),
        new("Cookie2", "def", "localhost")
    ]);
})
```

### Stopping the engine

The engine stops when the 
* The total number of pages to crawl is reached.
* 2 minutes have passed since the last job was added


## Extensibility

The library is designed to be extensible. It's possible to add custom `IParser`, `IScheduler` and `ICrawler` implementations.

using the engine builder it's possible to add custom implementations

```csharp
.ConfigureEngine(options =>
{
    options.WithCrawler(new MyCustomCrawler());
    options.WithScheduler(new MyCustomScheduler());
    options.WithParser(new MyCustomParser());
})
```


### Custom Outputs

It's possible to add custom outputs by implementing the `ICrawlOutput` interface.

`ICrawlOutput.WriteAsync(JObject jObject, CancellationToken cancellationToken);` is called for each object that is scraped.
