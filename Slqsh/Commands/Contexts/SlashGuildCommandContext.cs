using Disqord;
using Microsoft.Extensions.DependencyInjection;

namespace Slqsh;

public class SlashGuildCommandContext : SlashCommandContext
{
    public SlashGuildCommandContext(IServiceScope serviceScope, ISlashCommandInteraction interaction) 
        : base(serviceScope, interaction)
    { }

    public new Snowflake GuildId => base.GuildId.Value;

    public new IMember Author => (IMember) base.Author;
}