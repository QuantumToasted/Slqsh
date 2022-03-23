using Disqord;
using Qmmands;

namespace Slqsh;

public sealed class SupportedFileExtensionsAttribute : SlashCommandParameterCheckAttribute
{
    public SupportedFileExtensionsAttribute(params string[] extensions)
    {
        if (extensions.Length == 0)
            throw new ArgumentException("Supported extensions array must not be empty.", nameof(extensions));

        Extensions = extensions;
    }

    public string[] Extensions { get; }

    public override ValueTask<CheckResult> CheckAsync(object argument, SlashCommandContext context)
    {
        var attachment = (IAttachment) argument;
        var split = attachment.FileName.Split('.');
        if (split.Length < 2)
            return Failure("The supplied attachment must have a file extension.");

        var extension = split[^1];
        return Extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase)
            ? Success()
            : Failure("The supplied attachment must be one of the following types: " +
                      string.Join(", ", Extensions.Select(x => Markdown.Code(x))));
    }
}
