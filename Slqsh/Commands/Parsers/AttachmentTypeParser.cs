using Disqord;
using Qmmands;

namespace Slqsh;

public sealed class AttachmentTypeParser : SlashCommandTypeParser<IAttachment>
{
    public override ValueTask<TypeParserResult<IAttachment>> ParseAsync(Parameter parameter, string value, SlashCommandContext context)
    {
        return Snowflake.TryParse(value, out var attachmentId) 
            ? Success(context.Entities.Attachments[attachmentId])
            : Failure($"The supplied string \"{value}\" was not a properly formatted Discord ID.");
    }
}