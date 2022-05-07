using Disqord;
using Qmmands;

namespace Slqsh;

public sealed class GuildChannelTypeParser<TChannel> : SlashGuildCommandTypeParser<TChannel>
    where TChannel : IGuildChannel
{
    public override ValueTask<TypeParserResult<TChannel>> ParseAsync(Parameter parameter, string value, SlashGuildCommandContext context)
    {
        return Snowflake.TryParse(value, out var channelId)
            ? Success((TChannel) context.Entities.Channels[channelId])
            : Failure($"The supplied string \"{value}\" was not a properly formatted Discord ID.");
    }
}