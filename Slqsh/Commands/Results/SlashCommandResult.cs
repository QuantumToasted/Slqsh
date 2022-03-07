using System.ComponentModel;
using System.Runtime.CompilerServices;
using Qmmands;

namespace Slqsh;

public abstract class SlashCommandResult : CommandResult
{
    protected SlashCommandResult(SlashCommandContext context)
    {
        Context = context;
    }

    public SlashCommandContext Context { get; }

    public override bool IsSuccessful => true;

    public abstract Task ExecuteAsync();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual TaskAwaiter GetAwaiter()
        => ExecuteAsync().GetAwaiter();
}
