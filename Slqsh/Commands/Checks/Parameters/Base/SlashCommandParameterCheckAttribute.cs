using Qmmands;

namespace Slqsh;

public abstract class SlashCommandParameterCheckAttribute : ParameterCheckAttribute
{
    public abstract ValueTask<CheckResult> CheckAsync(object argument, SlashCommandContext context);

    public sealed override ValueTask<CheckResult> CheckAsync(object argument, CommandContext _)
    {
        var context = (SlashCommandContext)_;
        return CheckAsync(argument, context);
    }
}
