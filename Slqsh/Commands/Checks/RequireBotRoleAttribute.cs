using Disqord;
using Disqord.Gateway;
using Qmmands;

namespace Slqsh;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireBotRoleAttribute : SlashGuildCommandCheckAttribute
{
    public RequireBotRoleAttribute(ulong requiredRoleId)
    {
        RequiredRoleId = requiredRoleId;
    }

    public Snowflake RequiredRoleId { get; }

    public override ValueTask<CheckResult> CheckAsync(SlashGuildCommandContext context)
    {
        var roleIds = context.Client.GetMember(context.GuildId, context.Client.CurrentUser.Id).RoleIds;
        if (roleIds.Contains(RequiredRoleId))
            return Success();

        return Failure($"This command can only be used if I have the role with the ID {RequiredRoleId}.");
    }
}
