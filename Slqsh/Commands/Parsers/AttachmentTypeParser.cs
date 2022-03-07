using Disqord;
using Qmmands;

namespace Slqsh;

public sealed class AttachmentTypeParser : SlashCommandTypeParser<IAttachment>
{
    public override ValueTask<TypeParserResult<IAttachment>> ParseAsync(Parameter parameter, string value, SlashCommandContext context)
    {
        var attachmentId = Snowflake.Parse(value);
        return Success(context.Entities.Attachments[attachmentId]);
    }
}