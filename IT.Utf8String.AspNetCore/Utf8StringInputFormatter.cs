using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace IT.AspNetCore.Mvc.Formatters;

public class Utf8StringInputFormatter : InputFormatter
{
    private static readonly MediaTypeHeaderValue _mediaType = new("text/plain") { Charset = "utf-8" };

    public Utf8StringInputFormatter()
    {
        SupportedMediaTypes.Add(_mediaType);
    }

    protected override bool CanReadType(Type type) => type == typeof(Utf8String) || type == typeof(ReadOnlyUtf8String);

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    {
        var request = context.HttpContext.Request;

        var length = request.ContentLength ?? throw new InvalidOperationException("ContentLength is null");
        if (length > int.MaxValue) throw new InvalidOperationException("ContentLength is too large");

        var bytes = await ReadAsync(length, request.Body);

        if (bytes != null)
        {
            var modelType = context.ModelType;
            if (modelType == typeof(Utf8String)) return InputFormatterResult.Success(new Utf8String(bytes));
            if (modelType == typeof(ReadOnlyUtf8String)) return InputFormatterResult.Success(new ReadOnlyUtf8String(bytes));
        }

        return InputFormatterResult.Failure();
    }

    private static async Task<byte[]> ReadAsync(long length, Stream stream)
    {
        var bytes = new byte[length];
        var memory = bytes.AsMemory();
        int readed;

        do
        {
            readed = await stream.ReadAsync(memory);

            memory = memory[readed..];

        } while (memory.Length > 0);

        return bytes;
    }
}