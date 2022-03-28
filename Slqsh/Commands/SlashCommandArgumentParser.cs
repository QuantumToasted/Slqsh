using Qmmands;

namespace Slqsh;

public sealed class SlashCommandArgumentParser : IArgumentParser
{
    public static readonly SlashCommandArgumentParser Instance = new();

    public ValueTask<ArgumentParserResult> ParseAsync(CommandContext _)
    {
        var context = (SlashCommandContext)_;
        var command = context.Command;

        var arguments = new Dictionary<Parameter, object>(command.Parameters.Count);

        foreach (var parameter in command.Parameters)
        {
            var hasValue = context.Options.TryGetValue(parameter.Name, out var value);
            if (!parameter.IsOptional)
            {
                if (!hasValue)
                    throw new Exception($"Parameter `{parameter.Name}` is not optional and its option does not have value.");

                arguments[parameter] = value.Value.ToString();
            }
            else if (hasValue)
            {
                arguments[parameter] = value.Value.ToString();
            }
        }

        return new ValueTask<ArgumentParserResult>(new DefaultArgumentParserResult(command, arguments));
    }
}