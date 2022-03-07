using Disqord;
using Newtonsoft.Json;

namespace Slqsh;

public sealed class JsonSlashCommandOptionChoice
{
    private JsonSlashCommandOptionChoice()
    { }

    public JsonSlashCommandOptionChoice(LocalSlashCommandOptionChoice choice)
    {
        Name = choice.Name.Value;
        Value = choice.Value.Value.ToString();
    }

    public JsonSlashCommandOptionChoice(ISlashCommandOptionChoice choice)
    {
        Name = choice.Name;
        Value = choice.Value.ToString();
    }

    [JsonProperty("name")]
    public string Name { get; private set; }

    [JsonProperty("value")]
    public string Value { get; private set; }

    public override bool Equals(object obj)
    {
        if (obj is not JsonSlashCommandOptionChoice other)
            return false;

        if (!other.Name.Equals(Name) ||
            !other.Value.Equals(Value))
        {
            return false;
        }

        return true;
    }

    public LocalSlashCommandOptionChoice ToLocalChoice()
    {
        return new LocalSlashCommandOptionChoice()
            .WithName(Name)
            .WithValue(Value);
    }

}