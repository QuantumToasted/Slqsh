using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Qmmands;
using System.Text.RegularExpressions;

namespace Slqsh;

public class SlashCommandService : IHostedService
{
    private static readonly Regex SlashCommandValidationRegex = new(@"^[\w-]{1,32}$", RegexOptions.Compiled);
    private readonly Dictionary<Type, AutoCompleteResolver> _autoCompleteResolvers;

    public SlashCommandService(IServiceProvider services, SlashCommandServiceConfiguration configuration, 
        DiscordClientBase client, ILogger<SlashCommandService> logger)
    {
        Services = services;
        Client = client;
        Configuration = configuration;
        _autoCompleteResolvers = new();
        Logger = logger;
        Commands = new CommandService(new CommandServiceConfiguration
        {
            DefaultArgumentParser = SlashCommandArgumentParser.Instance
        });

        Commands.CommandExecuted += OnCommandExecuted;
        Client.InteractionReceived += OnInteractionReceived;
    }
    
    protected IServiceProvider Services { get; }

    protected DiscordClientBase Client { get; }

    protected SlashCommandServiceConfiguration Configuration { get; }

    protected ILogger Logger { get; }

    public CommandService Commands { get; protected set; }

    public IReadOnlyDictionary<string, Command> RawCommands { get; protected set; }

    public virtual ValueTask RegisterInternalCommandsAsync()
    {
        foreach (var assembly in Configuration.SlashCommandModuleAssemblies)
        {
            try
            {
                var modules = Commands.AddModules(assembly);

                foreach (var module in modules)
                {
                    if (module.Aliases.Count > 0)
                    {
                        if (string.IsNullOrWhiteSpace(module.Description))
                            throw new Exception($"Module `{module.Name}` must have a description set if it is marked with a GroupAttribute.");

                        foreach (var alias in module.Aliases)
                        {
                            if (!SlashCommandValidationRegex.IsMatch(alias))
                                throw new Exception($"Module `{module.Name}` has a GroupAttribute/alias must pass Regex validation ({SlashCommandValidationRegex}).");

                            if (alias.Any(char.IsUpper))
                                throw new Exception($"Module `{module.Name}` has a GroupAttribute/alias must not contain uppercase characters.");
                        }
                    }

                    if (string.IsNullOrWhiteSpace(module.Description) && module.Aliases.Count > 0)
                        throw new Exception($"Module `{module.Name}` must have a description set if it is marked with a GroupAttribute.");

                    foreach (var subModule in CommandUtilities.EnumerateAllSubmodules(module))
                    {
                        if (string.IsNullOrWhiteSpace(subModule.Description) && subModule.Aliases.Count > 0)
                            throw new Exception($"Submodule `{subModule.Name}` in module `{module.Name}` must have a description set if it is marked with a GroupAttribute.");
                    }

                    foreach (var command in CommandUtilities.EnumerateAllCommands(module))
                    {
                        if (command.Aliases.Count > 1)
                            throw new Exception($"Command `{command.Name}` in module `{module.Name}` must not have more than one alias.");

                        if (!SlashCommandValidationRegex.IsMatch(command.Aliases[0]))
                            throw new Exception($"Command `{command.Name}` in module `{module.Name}`'s name must pass Regex validation ({SlashCommandValidationRegex}).");

                        if (command.Name.Any(char.IsUpper))
                            throw new Exception($"Command `{command.Name}` in module `{module.Name}`'s name must not contain uppercase characters.");

                        if (string.IsNullOrWhiteSpace(command.Description))
                            throw new FormatException($"Command `{command.Name}` in module `{module.Name}` must have a description set.");

                        foreach (var parameter in command.Parameters)
                        {
                            if (!SlashCommandValidationRegex.IsMatch(parameter.Name))
                                throw new Exception($"Parameter `{parameter.Name}` in command `{command.Name}` in module `{module.Name}`'s name must pass Regex validation ({SlashCommandValidationRegex}).");

                            if (parameter.Name.Any(char.IsUpper))
                                throw new Exception($"Parameter `{parameter.Name}` in command `{command.Name}` in module `{module.Name}`'s name must not contain uppercase characters.");

                            if (string.IsNullOrWhiteSpace(parameter.Description))
                                throw new Exception($"Parameter `{parameter.Name}` in command `{command.Name}` in module `{module.Name}` must have a description set.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to register internal local commands.");
            }
        }

        return ValueTask.CompletedTask;
    }

    public virtual async Task RegisterSlashCommandsAsync(CancellationToken cancellationToken)
    {
        var applicationId = Configuration.ApplicationId;
        if (applicationId == default)
        {
            var application = await Client.FetchCurrentApplicationAsync(cancellationToken: cancellationToken);
            applicationId = application.Id;
            Logger.LogWarning("You have not set your SlashCommandServiceConfiguration's ApplicationId. To prevent unnecessary REST requests and rate-limits, set it to {ApplicationId}.",
                applicationId.RawValue);
        }

        // 1. Register auto-complete resolvers
        // 2. Register Qmmands commands
        // 3. Register type parsers
        // 4. Convert registered commands into JSON commands
        // 5. Read from CommandDataFileName, and write to it if empty
        // 6. Compare stored commands (if any) to converted commands, generate add/modify/remove list
        // 7. Add/modify/remove slash commands
        // 8. If successful, write CommandDataFileName to file

        // 1. Register auto-complete resolvers
        foreach (var resolver in Services.GetServices<AutoCompleteResolver>())
        {
            _autoCompleteResolvers[resolver.ResolveForType] = resolver;
        }

        Logger.LogInformation("Registered {Count} slash command auto-complete resolvers.", _autoCompleteResolvers.Count);

        // 2. Register Qmmands commands
        try
        {
            await RegisterInternalCommandsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unable to register internal commands via Qmmands.");
            return;
        }

        var modules = Commands.TopLevelModules;

        Logger.LogInformation("Registered {ModuleCount} internal modules with {CommandCount} total commands.",
            modules.Count, Commands.GetAllCommands().Count);

        RawCommands = Commands.GetAllCommands().ToDictionary(command => command.Name);

        // 3. Register type parsers
        try
        {
            await AddTypeParsersAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unable to register slash command type parsers.");
            return;
        }

        // 4. Convert registered commands into JSON commands
        var slashCommands = new List<JsonSlashCommand>();
        // At the moment, slash commands can never be more then three layers deep. "one two three [args]" is the deepest, so only one submodule loop is necessary
        foreach (var module in modules)
        {
            if (module.Aliases.SingleOrDefault() is { } groupAlias)
            {
                var slashCommand = new LocalSlashCommand()
                    .WithName(groupAlias)
                    .WithDescription(module.Description); // command description is required for root commands with subcommands for some reason

                foreach (var command in module.Commands)
                {
                    var path = command.Name.Split(' ');
                    var option = new LocalSlashCommandOption()
                        .WithName(path[1])
                        .WithDescription(command.Description)
                        .WithType(SlashCommandOptionType.Subcommand);

                    foreach (var parameter in command.Parameters)
                    {
                        option.AddOption(parameter.ToSlashCommandOption(Configuration));
                    }

                    slashCommand.AddOption(option);
                }

                foreach (var subModule in module.Submodules)
                {
                    var subGroupAlias = subModule.Aliases.Single();

                    var option = new LocalSlashCommandOption()
                        .WithName(subGroupAlias)
                        .WithDescription(subModule.Description)
                        .WithType(SlashCommandOptionType.SubcommandGroup);

                    foreach (var subCommand in subModule.Commands)
                    {
                        var path = subCommand.Name.Split(' ');
                        var subOption = new LocalSlashCommandOption()
                            .WithName(path[2])
                            .WithDescription(subCommand.Description)
                            .WithType(SlashCommandOptionType.Subcommand);

                        foreach (var parameter in subCommand.Parameters)
                        {
                            subOption.AddOption(parameter.ToSlashCommandOption(Configuration));
                        }

                        option.AddOption(subOption);
                    }

                    slashCommand.AddOption(option);
                }

                slashCommands.Add(new JsonSlashCommand(slashCommand));
            }
            else
            {
                foreach (var command in module.Commands)
                {
                    var slashCommand = new LocalSlashCommand()
                        .WithName(command.Name)
                        .WithDescription(command.Description);

                    foreach (var parameter in command.Parameters)
                    {
                        slashCommand.AddOption(parameter.ToSlashCommandOption(Configuration));
                    }

                    slashCommands.Add(new JsonSlashCommand(slashCommand));
                }

                if (module.Submodules.Count > 0)
                {
                    Logger.LogError("Submodule {Submodule} was found in un-grouped module {Module}!",
                        module.Submodules[0].Name,
                        module.Name);

                    return;
                }
            }
        }

        // 5. Read from CommandDataFileName, and write to it if empty
        try
        {
            Directory.CreateDirectory(Configuration.CommandDataFilePath);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unable to create the specified directory {DirectoryName}.", Configuration.CommandDataFilePath);
            return;
        }

        var filePath = Path.Combine(Configuration.CommandDataFilePath, Configuration.CommandDataFileName);

        var storedCommands = new List<JsonSlashCommand>();
        if (!File.Exists(filePath))
        {
            Logger.LogWarning("{Path} was not found - remote slash command data will now be loaded.",
                filePath);

            var existingCommands = await Client.FetchGlobalApplicationCommandsAsync(applicationId, cancellationToken: cancellationToken);
            var existingSlashCommands = existingCommands.OfType<ISlashCommand>().ToList();

            if (existingSlashCommands.Count == 0)
            {
                Logger.LogWarning("No remote slash commands were found. All registered commands will be added.");
            }
            else
            {
                Logger.LogInformation("Found {Count} existing remote slash commands to save.", existingSlashCommands.Count);

                storedCommands = existingSlashCommands.Select(x => new JsonSlashCommand(x)).ToList();

                try
                {
                    var json = JsonConvert.SerializeObject(storedCommands);
                    await File.WriteAllTextAsync(filePath, json, cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Unable to save remote slash command data to {Path}.", filePath);
                    return;
                }

                Logger.LogInformation("Saved remote slash command data to {Path}.", filePath);
            }
        }
        else
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath, cancellationToken);
                storedCommands = JsonConvert.DeserializeObject<List<JsonSlashCommand>>(json);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to read local slash command data from existing file {Path}.", filePath);
            }
        }

        // 6. Compare stored commands (if any) to converted commands, generate add/modify/remove list
        var commandsToAdd = new List<JsonSlashCommand>();
        var commandsToModify = new List<JsonSlashCommand>();
        var commandsToDelete = new List<JsonSlashCommand>();

        if (storedCommands.Count == 0)
        {
            Logger.LogWarning("{Path} contains no commands (or no remote slash commands exist). Setting all slash commands instead of adding.", filePath);

            IReadOnlyList<IApplicationCommand> newCommands;
            try
            {
                newCommands = await Client.SetGlobalApplicationCommandsAsync(applicationId,
                    slashCommands.Select(x => x.ToLocalCommand()), cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to set global slash commands.");
                return;
            }

            Logger.LogInformation("Successfully set {Count} new global slash commands.", newCommands.Count);

            foreach (var command in slashCommands)
            {
                var newCommand = newCommands.First(x => x.Name.Equals(command.Name));
                command.Id = newCommand.Id;
            }

            try
            {
                var json = JsonConvert.SerializeObject(slashCommands);
                await File.WriteAllTextAsync(filePath, json, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to save local slash command data to {Path}.", filePath);
                return;
            }

            Logger.LogInformation("Saved local slash command data to {Path}.", filePath);
            return;
        }

        foreach (var command in slashCommands)
        {
            // New command
            if (storedCommands.FirstOrDefault(x => x.Name.Equals(command.Name)) is not { } existingCommand)
            {
                commandsToAdd.Add(command);
                continue;
            }

            // Existing command that needs to be updated
            if (!existingCommand.Equals(command))
            {
                command.Id = existingCommand.Id;
                commandsToModify.Add(command);
            }
        }

        foreach (var command in storedCommands)
        {
            // Stale/deleted command. Can't check against IDs because local commands don't have IDs
            if (slashCommands.All(x => !x.Name.Equals(command.Name)))
            {
                commandsToDelete.Add(command);
            }
        }

        Logger.LogInformation("Found {AddCount} slash commands to add, {ModifyCount} to modify, and {DeleteCount} to delete.",
            commandsToAdd.Count,
            commandsToModify.Count,
            commandsToDelete.Count);

        // 7. Add/modify/remove slash commands
        foreach (var command in commandsToAdd)
        {
            try
            {
                var newCommand = await Client.CreateGlobalApplicationCommandAsync(applicationId, command.ToLocalCommand(), cancellationToken: cancellationToken);
                command.Id = newCommand.Id;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to add global slash command {Name}.", command.Name);
                return;
            }
        }

        foreach (var command in commandsToModify)
        {
            try
            {
                var modifiedCommand = await Client.ModifyGlobalApplicationCommandAsync(applicationId, command.Id, x =>
                {
                    x.Description = command.Description;
                    x.IsEnabledByDefault = command.IsEnabledByDefault;
                    if (command.Options.Count > 0)
                    {
                        x.Options = command.Options.Select(y => y.ToLocalOption())
                            .ToList();
                    }
                }, cancellationToken: cancellationToken);

                command.Id = modifiedCommand.Id;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to modify global slash command {Name} ({Id}).", command.Name, command.Id);
                return;
            }
        }

        foreach (var command in commandsToDelete)
        {
            try
            {
                await Client.DeleteGlobalApplicationCommandAsync(applicationId, command.Id, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to delete global slash command {Name} ({Id}).", command.Name, command.Id);
                return;
            }
        }

        // 8. If there are any new or modified commands, write CommandDataFileName to file
        if (commandsToAdd.Count > 0 || commandsToModify.Count > 0)
        {
            try
            {
                var json = JsonConvert.SerializeObject(slashCommands);
                await File.WriteAllTextAsync(filePath, json, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to save local slash command data to {Path}.", filePath);
                return;
            }

            Logger.LogInformation("Saved local slash command data to {Path}.", filePath);
        }
    }

    public virtual ValueTask AddTypeParsersAsync()
    {
        Commands.AddTypeParser(new GuildChannelTypeParser<IGuildChannel>());
        Commands.AddTypeParser(new GuildChannelTypeParser<ITextChannel>());
        Commands.AddTypeParser(new GuildChannelTypeParser<IVoiceChannel>());
        Commands.AddTypeParser(new GuildChannelTypeParser<ICategoryChannel>());
        Commands.AddTypeParser(new GuildChannelTypeParser<IThreadChannel>());
        Commands.AddTypeParser(new UserTypeParser<IUser>());
        Commands.AddTypeParser(new UserTypeParser<IMember>());
        Commands.AddTypeParser(new RoleTypeParser());
        Commands.AddTypeParser(new AttachmentTypeParser());

        return ValueTask.CompletedTask;
    }

    public virtual async ValueTask HandleAutoCompleteAsync(IAutoCompleteInteraction interaction)
    {
        var path = interaction.GetFullPath();
        if (!RawCommands.TryGetValue(path, out var command))
        {
            Logger.LogWarning("Slash command {Path} does not have a valid command mapped to it.", path);
            return;
        }

        var optionToAutoComplete = interaction.GetOptionToAutoComplete();
        var parameter = command.Parameters.First(x => x.Name.Equals(optionToAutoComplete.Name));
        var autoCompleteType = parameter.Attributes.OfType<AutoCompleteAttribute>().First().OverrideAutoCompleteType
                               ?? parameter.Type;
        if (!_autoCompleteResolvers.TryGetValue(autoCompleteType, out var resolver))
        {
            Logger.LogWarning("Type {Type} does not have an auto-complete resolver defined for command {Path}.", autoCompleteType, path);
            return;
        }

        try
        {
            var choices = await resolver.GenerateChoicesAsync(interaction, optionToAutoComplete);
            if (choices.Count > 0)
            {
                await interaction.Response().AutoCompleteAsync(choices);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An exception occurred attempting to generate or send auto-complete choices for command {Path}.", path);
        }
    }

    public virtual async ValueTask HandleSlashCommandAsync(ISlashCommandInteraction interaction)
    {
        var path = interaction.GetFullPath();
        if (!RawCommands.TryGetValue(path, out var command))
        {
            Logger.LogWarning("Slash command {Path} does not have a valid command mapped to it.", path);
            return;
        }

        var scope = Services.CreateScope();
        var context = new SlashCommandContext(scope, interaction);

        var result = await command.ExecuteAsync(string.Empty, context);

        if (result is FailedResult failedResult)
        {
            var reason = FormatFailureReason(context, failedResult);
            if (string.IsNullOrWhiteSpace(reason))
                return;

            if (context.Response().HasResponded) // most likely a defer, or possibly a post-handling exception??
            {
                await context.Followup().SendAsync(new LocalInteractionFollowup()
                    .WithContent("This command failed to run. Below might be some reasons why:\n" +
                                 Markdown.CodeBlock(reason))
                    .WithIsEphemeral());
            }
            else
            {
                await context.Response().SendMessageAsync(new LocalInteractionMessageResponse()
                    .WithContent("This command failed to run. Below might be some reasons why:\n" +
                                 Markdown.CodeBlock(reason))
                    .WithIsEphemeral());
            }

            if (failedResult is CommandExecutionFailedResult executionFailedResult)
            {
                Logger.LogError(executionFailedResult.Exception,
                    "An unhandled exception occurred while processing step {Step} for command {Path}.",
                    executionFailedResult.CommandExecutionStep, path);
            }
        }
    }

    // Borrowed from Disqord.Bot, very barebones
    public virtual string FormatFailureReason(SlashCommandContext context, FailedResult result)
    {
        return result switch
        {
            CommandNotFoundResult => null,
            TypeParseFailedResult typeParseFailedResult => $"Type parse failed for parameter '{typeParseFailedResult.Parameter}':\n• {typeParseFailedResult.FailureReason}",
            ChecksFailedResult checksFailedResult => string.Join('\n', checksFailedResult.FailedChecks.Select(x => $"• {x.Result.FailureReason}")),
            ParameterChecksFailedResult parameterChecksFailedResult => $"Checks failed for parameter '{parameterChecksFailedResult.Parameter}':\n"
                                                                       + string.Join('\n', parameterChecksFailedResult.FailedChecks.Select(x => $"• {x.Result.FailureReason}")),
            _ => result.FailureReason
        };
    }

    private ValueTask OnCommandExecuted(object sender, CommandExecutedEventArgs e)
    {
        if (e.Result is not SlashCommandResult result)
            return ValueTask.CompletedTask;

        try
        {
            return new ValueTask(result.ExecuteAsync());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An exception occurred during post-command execution for command {CommandPath}.", e.Context.Command.Name);
            return ValueTask.CompletedTask;
        }
    }

    private ValueTask OnInteractionReceived(object sender, InteractionReceivedEventArgs e)
    {
        return e.Interaction switch
        {
            IAutoCompleteInteraction autoCompleteInteraction => HandleAutoCompleteAsync(autoCompleteInteraction),
            ISlashCommandInteraction slashCommandInteraction => HandleSlashCommandAsync(slashCommandInteraction),
            _ => ValueTask.CompletedTask
        };
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
        => RegisterSlashCommandsAsync(cancellationToken);

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}