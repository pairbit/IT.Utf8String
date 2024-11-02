using Microsoft.AspNetCore.Mvc.Formatters;
using System.Buffers;

namespace IT.AspNetCore.Mvc.Formatters;

public class Utf8StringOutputFormatter : OutputFormatter
{
    public Utf8StringOutputFormatter()
    {
        SupportedMediaTypes.Add(Utf8StringMediaType.TextPlainUtf8);
    }

    public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
    {
        var obj = context.Object;

        var writer = context.HttpContext.Response.BodyWriter;

        if (obj is Utf8String utf8String)
        {
            if (utf8String.Length > 0)
                writer.Write(utf8String.Span);
        }
        else if (obj is ReadOnlyUtf8String readOnlyUtf8String)
        {
            if (readOnlyUtf8String.Length > 0)
                writer.Write(readOnlyUtf8String.Span);
        }
        else
        {
            throw new NotSupportedException(obj != null ? $"Type {obj.GetType()} not supported" : "Object is null");
        }

        return writer.FlushAsync().AsTask();
    }

    protected override bool CanWriteType(Type? type) => type == typeof(Utf8String) || type == typeof(ReadOnlyUtf8String);
}