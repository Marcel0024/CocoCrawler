using CocoCrawler.Exceptions;

namespace CocoCrawler.Job.PageBrowserActions;

public class PageActionsBuilder
{
    internal List<PageAction> Actions { get; set; } = [];

    public PageActionsBuilder ScrollToEnd()
    {
        Actions.Add(new PageAction(PageActionType.ScrollToEnd));

        return this;
    }

    public PageActionsBuilder Wait(int milliseconds)
    {
        Actions.Add(new PageAction(PageActionType.Wait, milliseconds.ToString()));

        return this;
    }

    internal PageActions Build()
    {
        if (Actions.Count == 0)
        {
            throw new CocoCrawlerBuilderException($"A PageAction requires a purpose, try calling .{nameof(ScrollToEnd)}() or .{nameof(Wait)}().");
        }

        return new PageActions([.. Actions]);
    }
}
