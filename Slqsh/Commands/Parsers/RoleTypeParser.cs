using Disqord;
using Qmmands;

namespace Slqsh;

public sealed class RoleTypeParser : SlashGuildCommandTypeParser<IRole>
{
    public override ValueTask<TypeParserResult<IRole>> ParseAsync(Parameter parameter, string value, SlashGuildCommandContext context)
    {
        var roleId = Snowflake.Parse(value);
        var role = context.Entities.Roles[roleId];
        return Success(role);
    }
}