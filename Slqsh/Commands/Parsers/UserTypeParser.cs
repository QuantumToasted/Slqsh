using Disqord;
using Qmmands;

namespace Slqsh;

public sealed class UserTypeParser<TUser> : SlashGuildCommandTypeParser<TUser>
    where TUser : IUser
{
    public override ValueTask<TypeParserResult<TUser>> ParseAsync(Parameter parameter, string value, SlashGuildCommandContext context)
    {
        var userId = Snowflake.Parse(value);
        var user = context.Entities.Users[userId];
        return Success((TUser) user);
    }
}