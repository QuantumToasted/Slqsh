﻿using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Slqsh;

public abstract class SlashModuleBase : SlashModuleBase<SlashCommandContext>
{ }

public abstract class SlashModuleBase<TContext> : ModuleBase<TContext>
    where TContext : SlashCommandContext
{
    private ILogger _logger;

    protected virtual ILogger Logger
    {
        get
        {
            if (_logger != null || Context == null)
                return _logger;

            return _logger = Context.Services.GetRequiredService<ILoggerFactory>().CreateLogger($"Command '{Context.Command.Name}'");
        }
        set => _logger = value;
    }

    protected SlashCommandResponseResult Response(string text, bool isEphemeral = false)
        => Response(new LocalInteractionMessageResponse().WithContent(text).WithIsEphemeral(isEphemeral));

    protected SlashCommandResponseResult Response(LocalEmbed embed, bool isEphemeral = false)
        => Response(new LocalInteractionMessageResponse().WithEmbeds(embed).WithIsEphemeral(isEphemeral));

    protected SlashCommandResponseResult Response(string text, LocalEmbed embed, bool isEphemeral = false)
        => Response(new LocalInteractionMessageResponse().WithContent(text).WithEmbeds(embed).WithIsEphemeral(isEphemeral));

    protected SlashCommandResponseResult Response(LocalInteractionMessageResponse response)
        => new(Context, response);

    protected SlashCommandMenuResult Menu(MenuBase menu, bool isEphemeral = false)
        => new(Context, menu, isEphemeral);

    protected SlashCommandMenuResult View(ViewBase view, bool isEphemeral = false)
        => Menu(new SlashDefaultMenu(view, Context.Interaction, isEphemeral) { AuthorId = Context.Author.Id }, isEphemeral);

    protected SlashCommandMenuResult Pages(PageProvider pageProvider, bool isEphemeral = false)
        => View(new SlashPagedView(pageProvider), isEphemeral);

    protected SlashCommandMenuResult Pages(IEnumerable<Page> pages, bool isEphemeral = false)
        => Pages(new ListPageProvider(pages), isEphemeral);

    protected SlashCommandFollowupResult Followup(string text, bool isEphemeral = false)
        => Followup(new LocalInteractionFollowup().WithContent(text).WithIsEphemeral(isEphemeral));

    protected SlashCommandFollowupResult Followup(LocalEmbed embed, bool isEphemeral = false)
        => Followup(new LocalInteractionFollowup().WithEmbeds(embed).WithIsEphemeral(isEphemeral));

    protected SlashCommandFollowupResult Followup(string text, LocalEmbed embed, bool isEphemeral = false)
        => Followup(new LocalInteractionFollowup().WithContent(text).WithEmbeds(embed).WithIsEphemeral(isEphemeral));

    protected SlashCommandFollowupResult Followup(LocalInteractionFollowup followup)
        => new(Context, followup);

    protected SlashCommandDeferResult Defer(bool isEphemeral = false)
        => new(Context, isEphemeral);

    protected SlashCommandModalResult Modal(string title, string label, TextInputComponentStyle style, string preFilledValue = null)
        => Modal(title, new LocalTextInputComponent().WithLabel(label).WithPrefilledValue(preFilledValue).WithStyle(style).WithCustomId(Guid.NewGuid().ToString()));

    protected SlashCommandModalResult Modal(string title, params LocalTextInputComponent[] components)
    {
        if (components.Length == 0)
            throw new ArgumentException("At least one text input component must be specified.", nameof(components));

        foreach (var component in components)
        {
            if (!component.CustomId.HasValue)
                component.WithCustomId(Guid.NewGuid().ToString());
        }

        var rows = new List<LocalRowComponent>(components.Length);
        foreach (var component in components)
        {
            rows.Add(new LocalRowComponent().WithComponents(component));
        }

        return Modal(new LocalInteractionModalResponse().WithTitle(title).WithComponents(rows));
    }

    protected SlashCommandModalResult Modal(LocalInteractionModalResponse modal)
        => new(Context, modal);
}