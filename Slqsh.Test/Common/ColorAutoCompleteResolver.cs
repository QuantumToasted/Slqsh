using System.Reflection;
using Disqord;

namespace Slqsh.Test;

public class ColorAutoCompleteResolver : AutoCompleteResolver
{
    private static readonly IReadOnlyDictionary<string, Color> ColorMap;

    public ColorAutoCompleteResolver(IServiceProvider services) 
        : base(services)
    { }

    public override Type ResolveForType => typeof(Color);

    public override ValueTask<IList<LocalSlashCommandOptionChoice>> GenerateChoicesAsync(IAutoCompleteInteraction interaction, IAutoCompleteInteractionOption optionToAutoComplete)
    {
        // This isn't "efficient". it's just meant to be a proof of concept.
        var value = optionToAutoComplete.Value.ToString();

        if (string.IsNullOrWhiteSpace(value))
            return new(GenerateDefaultReturnValues());

        var matchingColors = ColorMap.Where(x => x.Key.Contains(value, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        if (matchingColors.Count > 0)
        {
            return new(matchingColors.Select(x => new LocalSlashCommandOptionChoice().WithName(x.Key).WithValue(x.Value.RawValue.ToString())).ToList());
        }

        return new(GenerateDefaultReturnValues());

        static IList<LocalSlashCommandOptionChoice> GenerateDefaultReturnValues()
            => ColorMap.Take(25).Select(x => new LocalSlashCommandOptionChoice().WithName(x.Key).WithValue(x.Value.RawValue.ToString())).ToList();
    }

    static ColorAutoCompleteResolver()
    {
        // Excludes Color.Random
        var properties = typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Where(x => x.PropertyType == typeof(Color) && !x.Name.Equals("Random"));
        ColorMap = properties.OrderBy(x => x.Name).ToDictionary(x => x.Name, x => (Color) x.GetValue(null)!);
    }
}