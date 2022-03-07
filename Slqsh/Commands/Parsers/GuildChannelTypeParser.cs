using Disqord;
using Qmmands;

namespace Slqsh;

public sealed class GuildChannelTypeParser<TChannel> : SlashGuildCommandTypeParser<TChannel>
    where TChannel : IGuildChannel
{
    public override ValueTask<TypeParserResult<TChannel>> ParseAsync(Parameter parameter, string value, SlashGuildCommandContext context)
    {
        var channelId = Snowflake.Parse(value);
        var channel = context.Entities.Channels[channelId];
        return Success((TChannel) channel);
    }
}