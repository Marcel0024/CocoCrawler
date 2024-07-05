using CocoCrawler.Job.PageBrowserActions;
using CocoCrawler.Job.PageTasks;
using CocoCrawler.Outputs;
using System.Collections.Immutable;

namespace CocoCrawler.Job;

public record PageCrawlJob(
    string Url,
    ImmutableArray<IPageCrawlTask> Tasks,
    ImmutableArray<ICrawlOutput> Outputs,
    PageActions? BrowserActions = null);
