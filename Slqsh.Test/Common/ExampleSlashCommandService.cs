using Disqord;
using Microsoft.Extensions.Logging;

namespace Slqsh.Test;

public sealed class ExampleSlashCommandService : SlashCommandService
{
    public ExampleSlashCommandService(IServiceProvider services, SlashCommandServiceConfiguration configuration, DiscordClientBase client, ILogger<SlashCommandService> logger) 
        : base(services, configuration, client, logger)
    { }

    public override ValueTask AddTypeParsersAsync()
    {
        Commands.AddTypeParser(new ColorTypeParser());
        return base.AddTypeParsersAsync();
    }
}