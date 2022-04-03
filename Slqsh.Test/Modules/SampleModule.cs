using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Qmmands;

namespace Slqsh.Test;

public class SampleModule : SlashModuleBase
{
    [Command("echo")]
    [Description("Echo some text back to yourself.")]
    public SlashCommandResult Echo(
        [Description("The text you want to hear.")]
            string text)
    {
        return Response(text);
    }

    [Command("color")]
    [Description("Return an embed with a specified color by name.")]
    public SlashCommandResult Color(
        [Description("The color you wish to see.")] 
        [AutoComplete]
            Color color)
    {
        return Response(new LocalEmbed().WithColor(color).WithDescription($"You picked: #{color.RawValue:X6}"));
    }

    [Command("menu")]
    [Description("Display a sample menu.")]
    public SlashCommandResult ExampleMenu()
    {
        const string alphabet = "abcdefghijklmnopqrstuvwxyz";
        var pages = alphabet.Select(x => new Page()
            .WithEmbeds(new LocalEmbed()
                .WithColor(Disqord.Color.Random)
                .WithTitle("What's your favorite letter of the alphabet?")
                .WithDescription(x.ToString())));

        return Pages(pages);
    }
}