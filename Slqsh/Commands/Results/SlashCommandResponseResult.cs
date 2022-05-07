using Disqord;

namespace Slqsh;

public class SlashCommandResponseResult : SlashCommandResult
{
    public SlashCommandResponseResult(SlashCommandContext context, LocalInteractionMessageResponse response)
        : base(context)
    {
        Response = response;
    }

    public LocalInteractionMessageResponse Response { get; }

    public override Task ExecuteAsync()
        => Context.Response().HasResponded
            ? Context.Response().ModifyMessageAsync(Response)
            : Context.Response().SendMessageAsync(Response);
}