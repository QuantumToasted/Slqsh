using Qmmands;

namespace Slqsh;

public abstract class SlashGuildCommandTypeParser<T> : SlashCommandTypeParser<T>
{
    public abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, SlashGuildCommandContext context);

    public sealed override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, SlashCommandContext context)
    {
        if (!context.GuildId.HasValue)
            return Failure("This command can only be executed within a server.");

        if (context is not SlashGuildCommandContext guildContext)
            throw new InvalidOperationException($"The {GetType().Name} only accepts a {nameof(SlashGuildCommandContext)}.");

        return ParseAsync(parameter, value, guildContext);
    }
}
