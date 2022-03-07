using Qmmands;

namespace Slqsh;

public abstract class SlashGuildCommandParameterCheckAttribute : SlashCommandParameterCheckAttribute
{
    public abstract ValueTask<CheckResult> CheckAsync(object argument, SlashGuildCommandContext context);

    public sealed override ValueTask<CheckResult> CheckAsync(object argument, SlashCommandContext context)
    {
        if (!context.GuildId.HasValue)
            return Failure("This command can only be executed within a server.");

        if (context is not SlashGuildCommandContext guildContext)
            throw new InvalidOperationException($"The {GetType().Name} only accepts a {nameof(SlashGuildCommandContext)}.");

        return CheckAsync(argument, guildContext);
    }
}