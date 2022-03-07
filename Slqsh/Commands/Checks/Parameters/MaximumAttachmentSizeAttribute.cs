using Disqord;
using Qmmands;

namespace Slqsh;

public enum FileSize
{
    KB,
    MB
}

public sealed class MaximumAttachmentSizeAttribute : SlashCommandParameterCheckAttribute
{
    public MaximumAttachmentSizeAttribute(double size, FileSize measure)
    {
        Size = size;
        Measure = measure;
    }

    public double Size { get; }

    public FileSize Measure { get; }

    public override ValueTask<CheckResult> CheckAsync(object argument, SlashCommandContext context)
    {
        var attachment = (IAttachment) argument;
        var maximumSize = Convert.ToInt64(Size * Measure switch
        {
            FileSize.KB => 1_000,
            FileSize.MB => 1_000_000,
            _ => throw new ArgumentOutOfRangeException()
        });

        return attachment.FileSize < maximumSize
            ? Success()
            : Failure($"The supplied attachment must be {Size:F}{Measure} in size or less.");
    }
}
