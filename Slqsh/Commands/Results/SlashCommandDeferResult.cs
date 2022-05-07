namespace Slqsh;

public class SlashCommandDeferResult : SlashCommandResult
{
    public SlashCommandDeferResult(SlashCommandContext context, bool isEphemeral)
        : base(context)
    {
        IsEphemeral = isEphemeral;
    }

    public bool IsEphemeral { get; }

    public override Task ExecuteAsync()
        => Context.Response().DeferAsync(isEphemeral: IsEphemeral);
}
