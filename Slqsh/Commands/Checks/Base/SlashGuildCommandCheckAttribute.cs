using Qmmands;

namespace Slqsh;

public abstract class SlashGuildCommandCheckAttribute : SlashCommandCheckAttribute
{
    public abstract ValueTask<CheckResult> CheckAsync(SlashGuildCommandContext context);

    public sealed override ValueTask<CheckResult> CheckAsync(SlashCommandContext context)
    {
        if (!context.GuildId.HasValue)
            return Failure("This command can only be executed within a server.");

        if (context is not SlashGuildCommandContext guildContext)
            throw new InvalidOperationException($"The {GetType().Name} only accepts a {nameof(SlashGuildCommandContext)}.");

        return CheckAsync(guildContext);
    }
}