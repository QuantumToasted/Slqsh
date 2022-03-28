using Qmmands;

namespace Slqsh;

public abstract class SlashCommandTypeParser<T> : TypeParser<T>
{
    public abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, SlashCommandContext context);

    public sealed override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, CommandContext _)
    {
        if (_ is not SlashCommandContext context)
            throw new InvalidOperationException($"The {GetType().Name} only accepts a {nameof(SlashCommandContext)}.");

        return ParseAsync(parameter, value, context);
    }
}