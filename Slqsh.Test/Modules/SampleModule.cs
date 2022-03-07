using Qmmands;

namespace Slqsh.Test.Modules;

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
}