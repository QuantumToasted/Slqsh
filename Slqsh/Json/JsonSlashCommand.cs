using Disqord;
using Newtonsoft.Json;
using Qommon;

namespace Slqsh;

#pragma warning disable CS0659
public sealed class JsonSlashCommand
#pragma warning restore CS0659
{
    private JsonSlashCommand()
    { }

    public JsonSlashCommand(LocalSlashCommand command)
    {
        Name = command.Name.Value;
        IsEnabledByDefault = !command.IsEnabledByDefault.HasValue || command.IsEnabledByDefault.Value;
        Description = command.Description.Value;
        Options = (command.Options.GetValueOrDefault() ?? new List<LocalSlashCommandOption>()).Select(x => new JsonSlashCommandOption(x)).ToList();
    }

    public JsonSlashCommand(ISlashCommand command)
    {
        Id = command.Id;
        Name = command.Name;
        IsEnabledByDefault = command.IsEnabledByDefault;
        Description = command.Description;
        Options = command.Options.Select(x => new JsonSlashCommandOption(x)).ToList();
    }

    [JsonProperty("id")]
    public ulong Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; private set; }

    [JsonProperty("enabledByDefault")]
    public bool IsEnabledByDefault { get; private set; }

    [JsonProperty("description")]
    public string Description { get; private set; }

    [JsonProperty("options")]
    public IReadOnlyList<JsonSlashCommandOption> Options { get; private set; }

    public override bool Equals(object obj)
    {
        if (obj is not JsonSlashCommand other)
            return false;

        // Don't compare ID

        if (!other.Name.Equals(Name) ||
            other.IsEnabledByDefault != IsEnabledByDefault ||
            !other.Description.Equals(Description) ||
            other.Options.Count != Options.Count)
        {
            return false;
        }

        for (var i = 0; i < other.Options.Count; i++)
        {
            if (!other.Options[i].Equals(Options[i]))
                return false;
        }

        return true;
    }

    public LocalSlashCommand ToLocalCommand()
    {
        var command = new LocalSlashCommand()
            .WithName(Name)
            .WithIsEnabledByDefault(IsEnabledByDefault)
            .WithDescription(Description);

        if (Options.Count > 0)
            command.WithOptions(Options.Select(x => x.ToLocalOption()));

        return command;
    }
}