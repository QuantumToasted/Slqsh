using Disqord;
using Qmmands;

namespace Slqsh;

public sealed class UserTypeParser<TUser> : SlashGuildCommandTypeParser<TUser>
    where TUser : IUser
{
    public override ValueTask<TypeParserResult<TUser>> ParseAsync(Parameter parameter, string value, SlashGuildCommandContext context)
    {
        return Snowflake.TryParse(value, out var userId)
            ? Success((TUser) context.Entities.Users[userId])
            : Failure($"The supplied string \"{value}\" was not a properly formatted Discord ID.");
    }
}