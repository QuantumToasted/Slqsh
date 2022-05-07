using System.ComponentModel;
using System.Runtime.CompilerServices;
using Disqord;
using Disqord.Utilities.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Slqsh;

public class SlashCommandModalResult : SlashCommandResult
{
    private readonly SlashCommandService _service;

    public SlashCommandModalResult(SlashCommandContext context, LocalInteractionModalResponse modal) 
        : base(context)
    {
        _service = context.Services.GetRequiredService<SlashCommandService>();

        Modal = modal;

        if (!Modal.CustomId.HasValue)
            Modal.WithCustomId(Guid.NewGuid().ToString());
    }

    public LocalInteractionModalResponse Modal { get; }

    internal Cts Cts { get; private set; }

    internal IModalSubmitInteraction SubmittedModal { get; set; }

    public override Task ExecuteAsync()
        => Context.Response().SendModalAsync(Modal);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public new virtual TaskAwaiter<IModalSubmitInteraction> GetAwaiter()
        => WaitForModalCompletionAsync().GetAwaiter();

    private async Task<IModalSubmitInteraction> WaitForModalCompletionAsync()
    {
        _service.ModalResponses[Modal.CustomId.Value] = this;
        Cts = new Cts(TimeSpan.FromMinutes(14)); // slightly less than 15, the interaction token expiry

        await Context.Response().SendModalAsync(Modal);

        try
        {
            await Task.Delay(-1, Cts.Token);
        }
        catch (OperationCanceledException)
        { }

        return SubmittedModal;
    }
}