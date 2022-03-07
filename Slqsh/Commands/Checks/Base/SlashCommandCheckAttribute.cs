using Qmmands;

namespace Slqsh;

public abstract class SlashCommandCheckAttribute : CheckAttribute
{
    public abstract ValueTask<CheckResult> CheckAsync(SlashCommandContext context);

    public sealed override ValueTask<CheckResult> CheckAsync(CommandContext _)
    {
        var context = (SlashCommandContext)_;
        return CheckAsync(context);
    }
}