using System.Reflection;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Gateway;
using Disqord.Rest;

namespace Slqsh;

public class SlashDefaultMenu : MenuBase
{
    public SlashDefaultMenu(ViewBase view, ISlashCommandInteraction interaction, bool isEphemeral)
        : base(view)
    {
        Interaction = interaction;
        IsEphemeral = isEphemeral;
    }

    public ISlashCommandInteraction Interaction { get;  }

    public bool IsEphemeral { get; }

    public IUserMessage Message { get; protected set; }

    public Snowflake? AuthorId { get; set; }

    protected override async ValueTask<Snowflake> InitializeAsync(CancellationToken cancellationToken)
    {
        ValidateView();
        await View.UpdateAsync().ConfigureAwait(false);

        var messageId = MessageId;
        if (messageId != default)
            return messageId;

        if (!Interaction.Response().HasResponded)
            await Interaction.Response().DeferAsync(isEphemeral: IsEphemeral, cancellationToken: cancellationToken);

        var localMessage = View.ToLocalMessage();

        Message = await Interaction.Followup().SendAsync(new LocalInteractionFollowup()
            .WithContent(localMessage.Content)
            .WithEmbeds(localMessage.Embeds)
            .WithComponents(localMessage.Components)
            .WithAttachments(localMessage.Attachments)
            .WithIsEphemeral(IsEphemeral), cancellationToken: cancellationToken);

        // MessageId is not settable, even here
        var messageIdProperty = typeof(MenuBase).GetProperty("MessageId", BindingFlags.Public | BindingFlags.Instance);
        messageIdProperty!.SetValue(this, Message.Id);

        return Message.Id;
    }

    protected override ValueTask<bool> CheckInteractionAsync(InteractionReceivedEventArgs e)
    {
        if (IsEphemeral)
            return new(true);

        var authorId = AuthorId;
        if (authorId == null)
            return new(true);

        return new(e.AuthorId == authorId);
    }
}
