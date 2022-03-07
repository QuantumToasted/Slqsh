using Qmmands;

namespace Slqsh;

public abstract class SlashCommandTypeParser<T> : TypeParser<T>
{
    public abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, SlashCommandContext context);

    public sealed override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, CommandContext _)
    {
        var context = (SlashCommandContext)_;
        return ParseAsync(parameter, value, context);
    }
}