using Qmmands;

namespace Slqsh;

public abstract class SlashCommandCheckAttribute : CheckAttribute
{
    public abstract ValueTask<CheckResult> CheckAsync(SlashCommandContext context);

    public sealed override ValueTask<CheckResult> CheckAsync(CommandContext _)
    {
        if (_ is not SlashCommandContext context)
            throw new InvalidOperationException($"The {GetType().Name} only accepts a {nameof(SlashCommandContext)}.");

        return CheckAsync(context);
    }
}