using Disqord;
using Qmmands;

namespace Slqsh;

public sealed class RoleTypeParser : SlashGuildCommandTypeParser<IRole>
{
    public override ValueTask<TypeParserResult<IRole>> ParseAsync(Parameter parameter, string value, SlashGuildCommandContext context)
    {
        return Snowflake.TryParse(value, out var roleId)
            ? Success(context.Entities.Roles[roleId])
            : Failure($"The supplied string \"{value}\" was not a properly formatted Discord ID.");
    }
}