using Disqord;
using Disqord.Gateway;
using Qmmands;

namespace Slqsh;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireBotPermissionsAttribute : SlashGuildCommandCheckAttribute
{
    public RequireBotPermissionsAttribute(Permission requiredPermissions)
    {
        RequiredPermissions = requiredPermissions;
    }

    public Permission RequiredPermissions { get; }

    public override ValueTask<CheckResult> CheckAsync(SlashGuildCommandContext context)
    {
        var permissions = context.Client.GetMember(context.GuildId, context.Client.CurrentUser.Id).GetPermissions();
        return permissions.Has(RequiredPermissions)
            ? Success()
            : Failure("I am missing the following required server permissions for you to use this command: " +
                      $"{RequiredPermissions & ~permissions}");
    }
}
