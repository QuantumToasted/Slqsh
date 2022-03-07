using Disqord;
using Disqord.Gateway;
using Qmmands;

namespace Slqsh;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireAuthorRoleAttribute : SlashGuildCommandCheckAttribute
{
    public RequireAuthorRoleAttribute(ulong requiredRoleId)
    {
        RequiredRoleId = requiredRoleId;
    }

    public Snowflake RequiredRoleId { get; }

    public override ValueTask<CheckResult> CheckAsync(SlashGuildCommandContext context)
    {
        var roleIds = context.Author.RoleIds;
        if (roleIds.Contains(RequiredRoleId))
            return Success();

        return Failure($"This command can only be used by members having the role with ID {RequiredRoleId}.");
    }
}
