using Disqord;
using Disqord.Gateway;
using Qmmands;

namespace Slqsh;

[Flags]
public enum HierarchyType
{
    Author = 1,
    Bot = 2,
    All = Author | Bot
}

public sealed class RequireHierarchyAttribute : SlashGuildCommandParameterCheckAttribute
{
    public RequireHierarchyAttribute()
        : this(HierarchyType.All)
    { }

    public RequireHierarchyAttribute(HierarchyType hierarchyType)
    {
        HierarchyType = hierarchyType;
    }

    public HierarchyType HierarchyType { get; }

    public override ValueTask<CheckResult> CheckAsync(object argument, SlashGuildCommandContext context)
    {
        if (HierarchyType.HasFlag(HierarchyType.Author))
        {
            var (placeholder, pronoun) = CheckHierarchy(context.Author, argument);

            if (!string.IsNullOrWhiteSpace(placeholder))
                return Failure($"You must be higher in the role hierarchy than {placeholder} to use {pronoun} as an argument for this command.");
        }

        if (HierarchyType.HasFlag(HierarchyType.Bot))
        {
            var (placeholder, pronoun) = CheckHierarchy(context.Client.GetMember(context.GuildId, context.Client.CurrentUser.Id), argument);

            if (!string.IsNullOrWhiteSpace(placeholder))
                return Failure($"I must be higher in the role hierarchy than {placeholder} to use {pronoun} as an argument for this command.");
        }

        return Success();

        static (string Placeholder, string Pronoun) CheckHierarchy(IMember member, object argument)
        {
            if (argument is IRole targetRole)
                return member.GetHierarchy() > targetRole.Position ? (string.Empty, string.Empty) : (targetRole.Mention, "it");

            var targetMember = (IMember) argument;
            return member.GetHierarchy() > targetMember.GetHierarchy() ? (string.Empty, string.Empty) : (targetMember.Mention, "them");
        }
    }
}
