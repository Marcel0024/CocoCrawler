using System.Collections.Immutable;

namespace CocoCrawler.Job.PageBrowserActions;

public record PageActions(ImmutableArray<PageAction> Actions);