using Disqord;

namespace Slqsh;

public abstract class AutoCompleteResolver
{
    protected AutoCompleteResolver(IServiceProvider services)
    {
        Services = services;
    }

    public IServiceProvider Services { get; }

    public abstract Type ResolveForType { get; }

    public abstract ValueTask<IList<LocalSlashCommandOptionChoice>> GenerateChoicesAsync(IAutoCompleteInteraction interaction, IAutoCompleteInteractionOption optionToAutoComplete);
}
