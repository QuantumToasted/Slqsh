using Disqord;
using Qmmands;

namespace Slqsh;

public sealed class MemberTypeParser : SlashGuildCommandTypeParser<IMember>
{
    public override ValueTask<TypeParserResult<IMember>> ParseAsync(Parameter parameter, string value, SlashGuildCommandContext context)
    {
        var memberId = Snowflake.Parse(value);
        var user = context.Entities.Users[memberId];
        return Success((IMember) user);
    }
}