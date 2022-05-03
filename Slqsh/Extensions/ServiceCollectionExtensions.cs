using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Slqsh;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSlqsh(this IServiceCollection services, SlashCommandServiceConfiguration configuration = null)
        => services.AddSlqsh<SlashCommandService>(configuration);

    public static IServiceCollection AddSlqsh<TService>(this IServiceCollection services, SlashCommandServiceConfiguration configuration = null)
        where TService : SlashCommandService
    {
        configuration ??= SlashCommandServiceConfiguration.Default;
        services.AddSingleton(configuration);
        services.AddSingleton<TService>();
        services.AddSingleton<SlashCommandService, TService>(x => x.GetService<TService>());
        services.AddSingleton<IHostedService, TService>(x => x.GetService<TService>());

        foreach (var type in configuration.AutoCompleteResolverAssemblies.SelectMany(x => x.GetTypes())
                     .Where(x => typeof(AutoCompleteResolver).IsAssignableFrom(x) && !x.IsAbstract))
        {
            services.AddSingleton(type);
            services.AddSingleton(typeof(AutoCompleteResolver), x => x.GetService(type));
        }

        return services;
    }
}