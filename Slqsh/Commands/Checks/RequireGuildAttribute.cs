using Disqord;
using Qmmands;

namespace Slqsh;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireGuildAttribute : SlashGuildCommandCheckAttribute
{
    public RequireGuildAttribute()
    { }

    public RequireGuildAttribute(ulong requiredGuildId)
    {
        RequiredGuildId = requiredGuildId;
    }

    public Snowflake? RequiredGuildId { get; }

    public override ValueTask<CheckResult> CheckAsync(SlashGuildCommandContext context)
    {
        if (RequiredGuildId.HasValue && RequiredGuildId.Value != context.GuildId)
        {
            return Failure($"This command can only be used from within the server with the ID {RequiredGuildId.Value}");
        }

        return Success();
    }
}
