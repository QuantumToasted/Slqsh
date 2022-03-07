using System.Reflection;
using Disqord;

namespace Slqsh;

public class SlashCommandServiceConfiguration
{
    public static readonly SlashCommandServiceConfiguration Default = new();

    public IEnumerable<Assembly> SlashCommandModuleAssemblies { get; init; } = new[] {Assembly.GetEntryAssembly()};

    public IEnumerable<Assembly> AutoCompleteResolverAssemblies { get; init; } = new[] { Assembly.GetEntryAssembly() };

    public IDictionary<Type, SlashCommandOptionType> TypeMap { get; init; } = CommandExtensions.DefaultTypeMap;

    public IDictionary<Type, ChannelType[]> ChannelTypeMap { get; init; } = CommandExtensions.DefaultChannelTypeMap;

    public IEnumerable<Type> IntegerTypes { get; init; } = CommandExtensions.DefaultIntegerTypes;

    public IEnumerable<Type> NumberTypes { get; init; } = CommandExtensions.DefaultNumberTypes;

    public string CommandDataFilePath { get; init; } = "./";

    public string CommandDataFileName { get; init; } = "Commands.json";
}