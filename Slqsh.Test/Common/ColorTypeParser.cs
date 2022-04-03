using Disqord;
using Qmmands;

namespace Slqsh.Test;

public class ColorTypeParser : SlashCommandTypeParser<Color>
{
    // input is Color#RawValue.ToString()
    // Just a proof of concept
    public override ValueTask<TypeParserResult<Color>> ParseAsync(Parameter parameter, string value, SlashCommandContext context)
    {
        if (!int.TryParse(value, out var rawValue))
            return Failure("Invalid color specified. Please select a color name.");

        return Success(new Color(rawValue));
    }
}