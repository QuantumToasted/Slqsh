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
    {
        return Context.Followup().SendAsync(Followup);
    }
}
