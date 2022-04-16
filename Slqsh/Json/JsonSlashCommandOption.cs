using Disqord;
using Newtonsoft.Json;

namespace Slqsh;

#pragma warning disable CS0659
public sealed class JsonSlashCommandOption
#pragma warning restore CS0659
{
    private JsonSlashCommandOption()
    { }

    public JsonSlashCommandOption(LocalSlashCommandOption option)
    {
        Name = option.Name.Value;
        Type = option.Type.Value;
        Description = option.Description.Value;
        IsRequired = option.IsRequired.GetValueOrDefault();
        Choices = (option.Choices.GetValueOrDefault() ?? new List<LocalSlashCommandOptionChoice>()).Select(x => new JsonSlashCommandOptionChoice(x)).ToList();
        HasAutoComplete = option.HasAutoComplete.GetValueOrDefault();
        Options = (option.Options.GetValueOrDefault() ?? new List<LocalSlashCommandOption>()).Select(x => new JsonSlashCommandOption(x)).ToList();
        ChannelTypes = (option.ChannelTypes.GetValueOrDefault() ?? new List<ChannelType>()).ToList();
    }

    public JsonSlashCommandOption(ISlashCommandOption option)
    {
        Name = option.Name;
        Type = option.Type;
        Description = option.Description;
        IsRequired = option.IsRequired;
        Choices = option.Choices.Select(x => new JsonSlashCommandOptionChoice(x)).ToList();
        HasAutoComplete = option.HasAutoComplete;
        Options = option.Options.Select(x => new JsonSlashCommandOption(x)).ToList();
        ChannelTypes = option.ChannelTypes;
    }

    [JsonProperty("name")]
    public string Name { get; private set; }

    [JsonProperty("type")]
    public SlashCommandOptionType Type { get; private set; }

    [JsonProperty("description")]
    public string Description { get; private set; }

    [JsonProperty("isRequired")]
    public bool IsRequired { get; private set; }

    [JsonProperty("choices")]
    public IReadOnlyList<JsonSlashCommandOptionChoice> Choices { get; private set; }

    [JsonProperty("autoComplete")]
    public bool HasAutoComplete { get; private set; }

    [JsonProperty("options")]
    public IReadOnlyList<JsonSlashCommandOption> Options { get; private set; }

    [JsonProperty("channelTypes")]
    public IReadOnlyList<ChannelType> ChannelTypes { get; private set; }


    public override bool Equals(object obj)
    {
        if (obj is not JsonSlashCommandOption other)
            return false;

        if (!other.Name.Equals(Name) ||
            other.Type != Type ||
            !other.Description.Equals(Description) ||
            other.IsRequired != IsRequired ||
            other.HasAutoComplete != HasAutoComplete ||
            other.Choices.Count != Choices.Count ||
            other.Options.Count != Options.Count ||
            other.ChannelTypes.Count != ChannelTypes.Count)
        {
            return false;
        }

        for (var i = 0; i < other.Choices.Count; i++)
        {
            if (!other.Choices[i].Equals(Choices[i]))
                return false;
        }

        for (var i = 0; i < other.Options.Count; i++)
        {
            if (!other.Options[i].Equals(Options[i]))
                return false;
        }

        for (var i = 0; i < other.ChannelTypes.Count; i++)
        {
            if (other.ChannelTypes[i] != ChannelTypes[i])
                return false;
        }

        return true;
    }

    public LocalSlashCommandOption ToLocalOption()
    {
        var option = new LocalSlashCommandOption()
            .WithName(Name)
            .WithType(Type)
            .WithDescription(Description)
            .WithIsRequired(IsRequired)
            .WithHasAutoComplete(HasAutoComplete);

        if (Choices.Count > 0)
            option.WithChoices(Choices.Select(x => x.ToLocalChoice()));

        if (Options.Count > 0)
            option.WithOptions(Options.Select(x => x.ToLocalOption()));

        if (ChannelTypes.Count > 0)
            option.WithChannelTypes(ChannelTypes);

        return option;
    }
}