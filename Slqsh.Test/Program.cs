using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Slqsh;

// TODO: Write a more complete test suite, including parameters/options, more sub-commands, auto-completion, etc.

var host = new HostBuilder()
    .ConfigureHostConfiguration(config =>
    {
        config.AddEnvironmentVariables("SLQSH_");
    })
    .ConfigureLogging(logging =>
    {
        logging.AddSimpleConsole();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSlqsh(new SlashCommandServiceConfiguration()
        {
            DevelopmentGuild = context.HostingEnvironment.IsDevelopment() ? Snowflake.Parse(context.Configuration["DevGuild"]) : default 
        });
    })
    .ConfigureDiscordClient((context, client) =>
    {
        client.Token = context.Configuration["TOKEN"];
        client.Intents = GatewayIntents.Unprivileged;
    })
    .Build();

try
{
    host.Run();
}
catch (Exception ex)
{
    var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("SlqshTestBot");
    logger.LogCritical(ex, "An unhandled top level exception was thrown. Hosting has stopped.");
}
finally
{
    host.Dispose();
    Environment.Exit(-1);
}