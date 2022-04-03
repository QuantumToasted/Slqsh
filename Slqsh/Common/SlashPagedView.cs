using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;

namespace Slqsh;

public class SlashPagedView : PagedViewBase
{
    public ButtonViewComponent FirstPageButton { get; }

    public ButtonViewComponent PreviousPageButton { get; }

    public ButtonViewComponent NextPageButton { get; }

    public ButtonViewComponent LastPageButton { get; }

    public ButtonViewComponent StopButton { get; }

    public SlashPagedView(PageProvider pageProvider, LocalMessage templateMessage = null)
        : base(pageProvider, templateMessage)
    {
        FirstPageButton = new ButtonViewComponent(OnFirstPageButtonAsync)
        {
            Emoji = new LocalEmoji("⏮️"),
            Style = LocalButtonComponentStyle.Secondary
        };

        PreviousPageButton = new ButtonViewComponent(OnPreviousPageButtonAsync)
        {
            Emoji = new LocalEmoji("◀️"),
            Style = LocalButtonComponentStyle.Secondary
        };

        NextPageButton = new ButtonViewComponent(OnNextPageButtonAsync)
        {
            Emoji = new LocalEmoji("▶️"),
            Style = LocalButtonComponentStyle.Secondary
        };

        LastPageButton = new ButtonViewComponent(OnLastPageButtonAsync)
        {
            Emoji = new LocalEmoji("⏭️"),
            Style = LocalButtonComponentStyle.Secondary
        };

        StopButton = new ButtonViewComponent(OnStopButtonAsync)
        {
            Emoji = new LocalEmoji("⏹️"),
            Style = LocalButtonComponentStyle.Secondary
        };

        AddComponent(FirstPageButton);
        AddComponent(PreviousPageButton);
        AddComponent(NextPageButton);
        AddComponent(LastPageButton);
        AddComponent(StopButton);
    }

    protected virtual LocalMessage GetPagelessMessage()
        => new LocalMessage().WithContent("No pages to view.");

    protected virtual void ApplyPageIndex(Page page)
    {
        var indexText = $"Page {CurrentPageIndex + 1}/{PageProvider.PageCount}";
        var embed = page.Embeds.LastOrDefault();
        if (embed != null)
        {
            if (embed.Footer != null)
            {
                if (embed.Footer.Text == null)
                    embed.Footer.Text = indexText;
                else if (embed.Footer.Text.Length + indexText.Length + 3 <= LocalEmbedFooter.MaxTextLength)
                    embed.Footer.Text += $" | {indexText}";
            }
            else
            {
                embed.WithFooter(indexText);
            }
        }
        else
        {
            if (page.Content == null)
                page.Content = indexText;
            else if (page.Content.Length + indexText.Length + 1 <= LocalMessageBase.MaxContentLength)
                page.Content += $"\n{indexText}";
        }
    }

    public override async ValueTask UpdateAsync()
    {
        var previousPage = CurrentPage;
        await base.UpdateAsync().ConfigureAwait(false);

        var currentPage = CurrentPage;
        if (currentPage != null)
        {
            var currentPageIndex = CurrentPageIndex;
            var pageCount = PageProvider.PageCount;
            FirstPageButton.IsDisabled = currentPageIndex == 0;
            PreviousPageButton.IsDisabled = currentPageIndex == 0;
            NextPageButton.IsDisabled = currentPageIndex == pageCount - 1;
            LastPageButton.IsDisabled = currentPageIndex == pageCount - 1;

            if (previousPage != currentPage)
            {
                currentPage = currentPage.Clone();
                ApplyPageIndex(currentPage);
                CurrentPage = currentPage;
            }
        }
        else
        {
            TemplateMessage ??= GetPagelessMessage();
            FirstPageButton.IsDisabled = true;
            PreviousPageButton.IsDisabled = true;
            NextPageButton.IsDisabled = true;
            LastPageButton.IsDisabled = true;
        }
    }

    protected virtual ValueTask OnFirstPageButtonAsync(ButtonEventArgs e)
    {
        CurrentPageIndex = 0;
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask OnPreviousPageButtonAsync(ButtonEventArgs e)
    {
        CurrentPageIndex--;
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask OnNextPageButtonAsync(ButtonEventArgs e)
    {
        CurrentPageIndex++;
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask OnLastPageButtonAsync(ButtonEventArgs e)
    {
        CurrentPageIndex = PageProvider.PageCount - 1;
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask OnStopButtonAsync(ButtonEventArgs e)
    {
        if (Menu is SlashDefaultMenu defaultMenu)
        {
            var message = defaultMenu.Message;
            if (message != null)
                _ = message.ModifyAsync(x => x.Components = new List<LocalRowComponent>());
        }

        Menu.Stop();
        return ValueTask.CompletedTask;
    }
}