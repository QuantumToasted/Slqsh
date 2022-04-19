using System.ComponentModel;
using System.Runtime.CompilerServices;
using Disqord;

namespace Slqsh;

public class SlashCommandFollowupResult : SlashCommandResult
{
    public SlashCommandFollowupResult(SlashCommandContext context, LocalInteractionFollowup followup)
        : base(context)
    {
        Followup = followup;
    }

    public LocalInteractionFollowup Followup { get; }

    public override Task ExecuteAsync()
        => Context.Followup().SendAsync(Followup);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public new virtual TaskAwaiter<IUserMessage> GetAwaiter()
        => Context.Followup().SendAsync(Followup).GetAwaiter();
}
