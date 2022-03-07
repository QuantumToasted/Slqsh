namespace Slqsh;

public abstract class SlashGuildModuleBase : SlashGuildModuleBase<SlashGuildCommandContext>
{ }

[RequireGuild]
public abstract class SlashGuildModuleBase<TContext> : SlashModuleBase<TContext>
    where TContext : SlashGuildCommandContext
{ }