using System.Text;
using Disqord;
using Qmmands;

namespace Slqsh;

internal static class CommandExtensions
{
    public static readonly Dictionary<Type, ChannelType[]> DefaultChannelTypeMap = new()
    {
        [typeof(IGuildChannel)] = new[] { ChannelType.Category, ChannelType.Text, ChannelType.Voice, ChannelType.Category, ChannelType.PublicThread, ChannelType.PrivateThread },
        [typeof(ITextChannel)] = new[] { ChannelType.Text },
        [typeof(IVoiceChannel)] = new[] { ChannelType.Voice },
        [typeof(ICategoryChannel)] = new[] { ChannelType.Category },
        [typeof(IThreadChannel)] = new[] { ChannelType.PrivateThread, ChannelType.PublicThread }
    };

    public static readonly Dictionary<Type, SlashCommandOptionType> DefaultTypeMap = new()
    {
        [typeof(string)] = SlashCommandOptionType.String,
        [typeof(IRole)] = SlashCommandOptionType.String,
        [typeof(IMember)] = SlashCommandOptionType.User,
        [typeof(IGuildChannel)] = SlashCommandOptionType.Channel,
        [typeof(ITextChannel)] = SlashCommandOptionType.Channel,
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
        typeof(long),
        typeof(ulong),
        typeof(Snowflake),
        typeof(int?),
        typeof(uint?),
        typeof(byte?),
        typeof(sbyte?),
        typeof(short?),
        typeof(ushort?),
        typeof(long?),
        typeof(ulong?),
        typeof(Snowflake?)
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
            else throw new ArgumentException($"Channel type {parameter.Type.FullName} does not have a channel type map defined.", nameof(parameter));
        }

        if (parameter.Type.IsEnum)
        {
            foreach (var value in Enum.GetValues(parameter.Type))
            {
                option.AddChoice(new LocalSlashCommandOptionChoice()
                    .WithName(value.ToString())
                    .WithValue(value.ToString()));
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

        return optionType
               ?? throw new ArgumentException(
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

            if (type.IsEnum)
                return SlashCommandOptionType.String;

            return default;
        }
    }
}