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
        // This is only necessary as Disqord's DefaultMenu and PagedView are wholly intended for regular text command bots and break with interactions.
        if (Menu is DefaultMenu)
            throw new NotSupportedException("DefaultMenu and its implementations are not designed for interactions. Utilize SlashDefaultMenu instead.");

        if (Menu.View is PagedView)
            throw new NotSupportedException("PagedView and its implementations are not designed for interactions. Utilize SlashPagedView instead.");

        if (Menu is SlashDefaultMenu)
        {
            await Context.Client.StartMenuAsync(Context.ChannelId, Menu);
            return;
        }

        await Context.Response().DeferAsync(isEphemeral: IsEphemeral);

        var localMessage = Menu.View.ToLocalMessage();

        var message = await Context.Followup().SendAsync(new LocalInteractionFollowup()
            .WithContent(localMessage.Content)
            .WithEmbeds(localMessage.Embeds)
            .WithComponents(localMessage.Components)
            .WithAttachments(localMessage.Attachments)
            .WithIsEphemeral(IsEphemeral));

        // As we already have an instance of the menu, we need to set the message ID before sending it
        var messageIdProperty = typeof(MenuBase).GetProperty("MessageId", BindingFlags.Public | BindingFlags.Instance);
        messageIdProperty!.SetValue(Menu, message.Id);

        await Context.Client.StartMenuAsync(Context.ChannelId, Menu);
    }
}
