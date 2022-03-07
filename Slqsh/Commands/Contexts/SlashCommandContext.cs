using Disqord;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Slqsh;

public class SlashCommandContext : CommandContext, IDisposable
{
    private readonly IServiceScope _serviceScope;
    private IReadOnlyDictionary<string, ISlashCommandInteractionOption> _options;

    public SlashCommandContext(IServiceScope serviceScope, ISlashCommandInteraction interaction)
        : base(serviceScope.ServiceProvider)
    {
        _serviceScope = serviceScope;
        Interaction = interaction;
    }

    public ISlashCommandInteraction Interaction { get; }

    public IReadOnlyDictionary<string, ISlashCommandInteractionOption> Options
    {
        get
        {
            if (_options is not null)
                return _options;

            var options = Interaction.Options;
            if (options.Count != 1 ||
                options.Values.SingleOrDefault() is not { Type: SlashCommandOptionType.Subcommand or SlashCommandOptionType.SubcommandGroup } option)
            {
                return _options = options;
            }

            return option.Type == SlashCommandOptionType.Subcommand
                ? _options = option.Options
                : _options = option.Options.Values.Single().Options;
        }
    }

    public IUser Author => Interaction.Author;

    public Snowflake ChannelId => Interaction.ChannelId;

    public Snowflake? GuildId => Interaction.GuildId;

    public DiscordClientBase Client => (DiscordClientBase)Interaction.Client;

    public IApplicationCommandInteractionEntities Entities => Interaction.Entities;

    public InteractionResponseHelper Response() => Interaction.Response();

    public InteractionFollowupHelper Followup() => Interaction.Followup();

    public virtual void Dispose()
    {
        _serviceScope.Dispose();
    }

}