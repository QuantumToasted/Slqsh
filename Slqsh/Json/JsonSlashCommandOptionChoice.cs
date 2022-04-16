using Disqord;
using Newtonsoft.Json;

namespace Slqsh;

#pragma warning disable CS0659
public sealed class JsonSlashCommandOptionChoice
#pragma warning restore CS0659
{
    private JsonSlashCommandOptionChoice()
    { }

    public JsonSlashCommandOptionChoice(LocalSlashCommandOptionChoice choice)
    {
        Name = choice.Name.Value;
        Value = choice.Value.Value;
    }

    public JsonSlashCommandOptionChoice(ISlashCommandOptionChoice choice)
    {
        Name = choice.Name;
        Value = choice.Value;
    }

    [JsonProperty("name")]
    public string Name { get; private set; }

    [JsonProperty("value")]
    public object Value { get; private set; }

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
        var choice = new LocalSlashCommandOptionChoice()
            .WithName(Name);

        choice = Value switch
        {
            string s => choice.WithValue(s),
            int i => choice.WithValue(i),
            long l => choice.WithValue(l),
            double d => choice.WithValue(d),
            // Enum e => choice.WithValue(e.ToString()),
            _ => throw new ArgumentOutOfRangeException()
        };

        return choice;
    }
}