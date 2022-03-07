using Disqord;
using Qmmands;

namespace Slqsh;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireChannelAttribute : SlashCommandCheckAttribute
{
    public RequireChannelAttribute(ulong requiredChannelId)
    {
        RequiredChannelId = requiredChannelId;
    }

    public Snowflake RequiredChannelId { get; }

    public override ValueTask<CheckResult> CheckAsync(SlashCommandContext context)
    {
        if (context.ChannelId == RequiredChannelId)
            return Success();

        return Failure($"This command can only be used in the channel with the ID {RequiredChannelId}.");
    }
}
