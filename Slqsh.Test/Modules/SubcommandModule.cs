using Qmmands;

namespace Slqsh.Test;

[Group("build")]
[Description("Build some things.")]
// Developer's note: 99.99% certain these descriptions are NOT required. Or even shown. But valid to send when creating commands like these, so let's be safe. :-)
public class SubcommandModule : SlashGuildModuleBase
{
    [Command("car")]
    [Description("Build a car.")]
    public SlashCommandResult BuildACar()
        => Response("Do I look like I'm made of money to you?");

    [Command("house")]
    [Description("Build a house.")]
    public SlashCommandResult BuildAHouse()
        => Response("Heed my advice; the housing market isn't the greatest right now. Better wait to build one.");
}