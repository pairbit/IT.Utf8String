using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Buffers;
using System.Threading.Tasks;

namespace IT.AspNetCore.Mvc.Formatters;

public class Utf8StringOutputFormatter : OutputFormatter
{
    public Utf8StringOutputFormatter()
    {
        SupportedMediaTypes.Add(Net.Http.Headers.Utf8StringMediaTypeHeaderValue.TextPlainUtf8);
    }

    public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var obj = context.Object;

        var writer = context.HttpContext.Response.BodyWriter;

        if (obj is Utf8Memory utf8Memory)
        {
            if (utf8Memory.Length > 0)
                writer.Write(utf8Memory.Span);
        }
        else if (obj is ReadOnlyUtf8Memory readOnlyUtf8Memory)
        {
            if (readOnlyUtf8Memory.Length > 0)
                writer.Write(readOnlyUtf8Memory.Span);
        }
        else
        {
            throw new NotSupportedException(obj != null ? $"Type {obj.GetType()} not supported" : "Object is null");
        }

        return writer.FlushAsync().AsTask();
    }

    protected override bool CanWriteType(Type? type) => type == typeof(Utf8Memory) || type == typeof(ReadOnlyUtf8Memory);
}