using Disqord;
using Disqord.Gateway;
using Qmmands;

namespace Slqsh;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireAuthorPermissionsAttribute : SlashGuildCommandCheckAttribute
{
    public RequireAuthorPermissionsAttribute(Permission requiredPermissions)
    {
        RequiredPermissions = requiredPermissions;
    }

    public Permission RequiredPermissions { get; }

    public override ValueTask<CheckResult> CheckAsync(SlashGuildCommandContext context)
    {
        var permissions = context.Author.GetPermissions();
        return permissions.Has(RequiredPermissions)
            ? Success()
            : Failure("You are missing the following required server permissions to use this command: " +
                      $"{RequiredPermissions & ~permissions}");
    }
}