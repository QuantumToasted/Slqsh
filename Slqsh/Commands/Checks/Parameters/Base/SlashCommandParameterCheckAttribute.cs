using Qmmands;

namespace Slqsh;

public abstract class SlashCommandParameterCheckAttribute : ParameterCheckAttribute
{
    public abstract ValueTask<CheckResult> CheckAsync(object argument, SlashCommandContext context);

    public sealed override ValueTask<CheckResult> CheckAsync(object argument, CommandContext _)
    {
        if (_ is not SlashCommandContext context)
            throw new InvalidOperationException($"The {GetType().Name} only accepts a {nameof(SlashCommandContext)}.");

        return CheckAsync(argument, context);
    }
}
