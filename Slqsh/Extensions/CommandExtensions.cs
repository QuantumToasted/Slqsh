using System.Reflection;
using System.Text;
using Disqord;
using Qmmands;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace Slqsh;

internal static class CommandExtensions
{
    public static readonly Dictionary<Type, ChannelType[]> DefaultChannelTypeMap = new()
    {
        [typeof(IGuildChannel)] = new[] { ChannelType.Category, ChannelType.Text, ChannelType.Voice, ChannelType.Category, ChannelType.PublicThread, ChannelType.PrivateThread },
        [typeof(ITextChannel)] = new[] { ChannelType.Text },
        [typeof(IMessageGuildChannel)] = new[] { ChannelType.Text, ChannelType.PrivateThread, ChannelType.PublicThread },
        [typeof(IVoiceChannel)] = new[] { ChannelType.Voice },
        [typeof(ICategoryChannel)] = new[] { ChannelType.Category },
        [typeof(IThreadChannel)] = new[] { ChannelType.PrivateThread, ChannelType.PublicThread }
    };

    public static readonly Dictionary<Type, SlashCommandOptionType> DefaultTypeMap = new()
    {
        [typeof(string)] = SlashCommandOptionType.String,
        [typeof(IRole)] = SlashCommandOptionType.Role,
        [typeof(IUser)] = SlashCommandOptionType.User,
        [typeof(IMember)] = SlashCommandOptionType.User,
        [typeof(IGuildChannel)] = SlashCommandOptionType.Channel,
        [typeof(ITextChannel)] = SlashCommandOptionType.Channel,
        [typeof(IMessageGuildChannel)] = SlashCommandOptionType.Channel,
        [typeof(IVoiceChannel)] = SlashCommandOptionType.Channel,
        [typeof(ICategoryChannel)] = SlashCommandOptionType.Channel,
        [typeof(IThreadChannel)] = SlashCommandOptionType.Channel,
        [typeof(bool)] = SlashCommandOptionType.Boolean,
        [typeof(bool?)] = SlashCommandOptionType.Boolean,
        [typeof(IMentionableEntity)] = SlashCommandOptionType.Mentionable,
        [typeof(IAttachment)] = SlashCommandOptionType.Attachment
    };

    public static readonly Type[] DefaultIntegerTypes =
    {
        typeof(int),
        typeof(uint),
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(ushort),
        typeof(int?),
        typeof(uint?),
        typeof(byte?),
        typeof(sbyte?),
        typeof(short?),
        typeof(ushort?),
    };

    public static readonly Type[] DefaultNumberTypes =
    {
        typeof(decimal),
        typeof(double),
        typeof(decimal?),
        typeof(double?)
    };

    public static IAutoCompleteInteractionOption GetOptionToAutoComplete(this IAutoCompleteInteraction interaction)
    {
        return GetOption(interaction.Options);

        static IAutoCompleteInteractionOption GetOption(IReadOnlyDictionary<string, IAutoCompleteInteractionOption> options)
        {
            foreach (var (_, option) in options)
            {
                if (option.IsFocused)
                    return option;

                if (option.Options.Count > 0)
                    return GetOption(option.Options);
            }

            throw new InvalidOperationException("Auto-complete option name not successfully located.");
        }
    }

    public static string GetFullPath(this ISlashCommandInteraction interaction)
    {
        var builder = new StringBuilder(interaction.CommandName);

        foreach (var (name, option) in interaction.Options)
        {
            if (option.Type == SlashCommandOptionType.Subcommand)
            {
                builder.Append(' ').Append(name);
                break;
            }

            if (option.Type == SlashCommandOptionType.SubcommandGroup)
            {
                builder.Append(' ').Append(name);

                foreach (var (subName, subOption) in option.Options)
                {
                    if (subOption.Type == SlashCommandOptionType.Subcommand)
                    {
                        builder.Append(' ').Append(subName);
                        break;
                    }
                }
            }
        }

        return builder.ToString();
    }

    public static string GetFullPath(this IAutoCompleteInteraction interaction)
    {
        var builder = new StringBuilder(interaction.CommandName);

        foreach (var (name, option) in interaction.Options)
        {
            if (option.Type == SlashCommandOptionType.Subcommand)
            {
                builder.Append(' ').Append(name);
                break;
            }

            if (option.Type == SlashCommandOptionType.SubcommandGroup)
            {
                builder.Append(' ').Append(name);

                foreach (var (subName, subOption) in option.Options)
                {
                    if (subOption.Type == SlashCommandOptionType.Subcommand)
                    {
                        builder.Append(' ').Append(subName);
                        break;
                    }
                }
            }
        }

        return builder.ToString();
    }

    public static LocalSlashCommandOption ToSlashCommandOption(this Parameter parameter, SlashCommandServiceConfiguration configuration)
    {
        var option = new LocalSlashCommandOption()
            .WithName(parameter.Name)
            .WithDescription(parameter.Description)
            .WithType(parameter.GetSlashCommandOptionType(configuration))
            .WithIsRequired(!parameter.IsOptional);

        if (option.Type == SlashCommandOptionType.Channel)
        {
            if (configuration.ChannelTypeMap.TryGetValue(parameter.Type, out var channelTypes))
                option.WithChannelTypes(channelTypes);
            else throw new FormatException($"Channel type `{parameter.Type}` does not have a channel type map defined.");
        }

        if (parameter.HasChoices(out var choices))
        {
            if (choices.Count > Discord.Limits.ApplicationCommands.Options.MaxChoiceAmount)
                throw new FormatException($"Parameter `{parameter}` in command `{parameter.Command}` in module `{parameter.Command.Module}` contains too many choices. " +
                                          $"Expected ({Discord.Limits.ApplicationCommands.Options.MaxChoiceAmount}) choices or fewer, got ({choices.Count}).");

            if (!option.Type.Value.IsChoiceOptionType())
                throw new FormatException($"Parameter `{parameter}` in command `{parameter.Command}` in module `{parameter.Command.Module}` is not a valid choice type. " +
                                            "It must be a type which converts to a NUMBER, INTEGER, or STRING option type.");

            foreach (var choice in choices)
            {
                if (string.IsNullOrWhiteSpace(choice.Name))
                    throw new FormatException($"Parameter `{parameter}` in command `{parameter.Command}` in module `{parameter.Command.Module}` contains choices with " +
                                              "null or empty names.");

                if (choice.Name.Length > Discord.Limits.ApplicationCommands.Options.Choices.MaxNameLength)
                    throw new FormatException($"Parameter `{parameter}` in command `{parameter.Command}` in module `{parameter.Command.Module}` contains a choice " +
                                              $"`{choice.Name}` with length ({choice.Name.Length}) greater than the maximum length " +
                                              $"({Discord.Limits.ApplicationCommands.Options.Choices.MaxNameLength}).");

                if (choice.Value is null)
                    throw new FormatException($"Parameter `{parameter}` in command `{parameter.Command}` in module `{parameter.Command.Module}` contains a choice " +
                                              $"`{choice.Name}` with a null value.");

                var value = choice.Value;
                var valueUnderlyingType = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();
                var parameterUnderlyingType = Nullable.GetUnderlyingType(parameter.Type) ?? parameter.Type;
                if (/*(parameterUnderlyingType.IsPrimitive || parameterUnderlyingType == typeof(string)) &&*/
                    valueUnderlyingType != parameterUnderlyingType)
                {
                    throw new FormatException($"Parameter `{parameter}` in command `{parameter.Command}` in module `{parameter.Command.Module}` contains a choice " +
                                              $"`{choice.Name}` with mismatched primitive value type. Expected `{parameterUnderlyingType}`, got `{valueUnderlyingType}`");
                }

                var optionChoice = new LocalSlashCommandOptionChoice()
                    .WithName(choice.Name);

                optionChoice = option.Type.Value switch
                {
                    SlashCommandOptionType.String => optionChoice.WithValue(choice.Value.ToString()), // Can't use `as string` due to enums
                    SlashCommandOptionType.Integer => optionChoice.WithValue(Convert.ToInt32(choice.Value)),
                    SlashCommandOptionType.Number => optionChoice.WithValue(Convert.ToDouble(choice.Value)),
                    _ => throw new ArgumentOutOfRangeException(nameof(parameter), 
                        "Only String, Integer, and Number option type values can be used for choices.")
                };

                option.AddChoice(optionChoice);
            }
        }
        else
        {
            var parameterType = Nullable.GetUnderlyingType(parameter.Type) ?? parameter.Type;
            if (parameterType.IsEnum)
            {
                var exclusionAttribute = parameter.Attributes.OfType<ExcludeEnumValuesAttribute>().SingleOrDefault();
                if (exclusionAttribute is not null && exclusionAttribute.EnumType != parameterType)
                    throw new FormatException($"Enum type `{parameterType}` has enum value exclusions defined with a mismatched type " +
                                              $"`{exclusionAttribute.EnumType}`.");

                foreach (var value in Enum.GetNames(parameterType))
                {
                    if (exclusionAttribute?.ExcludedValueNames.Contains(value) == true)
                        continue;

                    option.AddChoice(new LocalSlashCommandOptionChoice()
                        .WithName(parameterType.GetEnumDescription(value) ?? value)
                        .WithValue(value));
                }
            }
        }

        if (parameter.Attributes.OfType<AutoCompleteAttribute>().Any())
        {
            option.WithHasAutoComplete();
        }

        return option;
    }

    public static SlashCommandOptionType GetSlashCommandOptionType(this Parameter parameter, SlashCommandServiceConfiguration configuration)
    {
        var optionType = GetOptionType(parameter.Type, configuration);

        if (!optionType.HasValue && parameter.Attributes.OfType<ParseFromStringAttribute>().Any())
        {
            optionType = SlashCommandOptionType.String;
        }

        return optionType ?? throw new ArgumentException(
            $"The type {parameter.Type.FullName} has not been defined to match an option type.",
            nameof(parameter));

        static SlashCommandOptionType? GetOptionType(Type type, SlashCommandServiceConfiguration configuration)
        {
            if (configuration.IntegerTypes.Contains(type))
                return SlashCommandOptionType.Integer;

            if (configuration.NumberTypes.Contains(type))
                return SlashCommandOptionType.Number;

            if (configuration.TypeMap.TryGetValue(type, out var foundType))
                return foundType;

            if (type.IsEnum || Nullable.GetUnderlyingType(type)?.IsEnum == true)
                return SlashCommandOptionType.String;

            return default;
        }
    }

    private static string GetEnumDescription(this Type enumType, string name)
    {
        return enumType.GetField(name)!.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }

    private static bool HasChoices(this Parameter parameter, out IList<ChoiceAttribute> choices)
    {
        choices = parameter.Attributes.OfType<ChoiceAttribute>().ToList();
        return choices.Count > 0;
    }

    private static bool IsChoiceOptionType(this SlashCommandOptionType type)
        => type is SlashCommandOptionType.String or SlashCommandOptionType.Number or SlashCommandOptionType.Integer;
}