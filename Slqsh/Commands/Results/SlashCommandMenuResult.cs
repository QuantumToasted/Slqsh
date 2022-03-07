using System.Reflection;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Slqsh;

public sealed class SlashCommandMenuResult : SlashCommandResult
{
    public SlashCommandMenuResult(SlashCommandContext context, MenuBase menu, bool isEphemeral)
        : base(context)
    {
        Menu = menu;
        IsEphemeral = isEphemeral;
    }

    public MenuBase Menu { get; }

    public bool IsEphemeral { get; }

    public override async Task ExecuteAsync()
    {
        await Context.Response().DeferAsync(isEphemeral: IsEphemeral);

        if (Menu.View is PagedView view)
        {
            await view.UpdateAsync();
        }

        var localMessage = Menu.View.ToLocalMessage();

        var message = await Context.Followup().SendAsync(new LocalInteractionFollowup()
            .WithContent(localMessage.Content)
            .WithEmbeds(localMessage.Embeds)
            .WithComponents(localMessage.Components)
            .WithAttachments(localMessage.Attachments)
            .WithIsEphemeral(IsEphemeral));

        // Need some way to set the message ID of the menu before it is sent, otherwise a new non-slash-command message will be sent.
        var messageId = typeof(MenuBase).GetProperty("MessageId", BindingFlags.Public | BindingFlags.Instance);
        messageId!.SetValue(Menu, message.Id);

        await Context.Client.StartMenuAsync(Context.ChannelId, Menu);
    }
}
