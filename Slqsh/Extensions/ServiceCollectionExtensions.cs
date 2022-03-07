using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Slqsh;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSlqsh(this IServiceCollection services, SlashCommandServiceConfiguration configuration = null)
    {
        configuration ??= SlashCommandServiceConfiguration.Default;
        services.AddSingleton(configuration);
        services.AddSingleton<SlashCommandService>();
        services.AddSingleton<IHostedService, SlashCommandService>(x => x.GetService<SlashCommandService>());

        foreach (var type in configuration.AutoCompleteResolverAssemblies.SelectMany(x => x.GetTypes())
                     .Where(x => typeof(AutoCompleteResolver).IsAssignableFrom(x) && !x.IsAbstract))
        {
            services.AddSingleton(type);
            services.AddSingleton(typeof(AutoCompleteResolver), x => x.GetService(type));
        }

        return services;
    }
}