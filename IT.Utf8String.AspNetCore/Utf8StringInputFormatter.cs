using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IT.AspNetCore.Mvc.Formatters;

public class Utf8StringInputFormatter : InputFormatter
{
    public Utf8StringInputFormatter()
    {
        SupportedMediaTypes.Add(Net.Http.Headers.Utf8StringMediaTypeHeaderValue.TextPlainUtf8);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var request = context.HttpContext.Request;

        var length = request.ContentLength ?? throw new InvalidOperationException("ContentLength is null");

        if (length == 0) return InputFormatterResult.Success(GetDefaultValueForType(context.ModelType));

        if (length > int.MaxValue) throw new InvalidOperationException("ContentLength is too large");

        var bytes = await ReadAsync(length, request.Body);

        if (bytes != null)
        {
            var modelType = context.ModelType;
            if (modelType == typeof(Utf8Memory)) return InputFormatterResult.Success(new Utf8Memory(bytes));
            if (modelType == typeof(ReadOnlyUtf8Memory)) return InputFormatterResult.Success(new ReadOnlyUtf8Memory(bytes));

            throw new NotSupportedException($"Type {modelType} not supported");
        }

        return InputFormatterResult.Failure();
    }

    protected override bool CanReadType(Type type) => type == typeof(Utf8Memory) || type == typeof(ReadOnlyUtf8Memory);

    protected override object? GetDefaultValueForType(Type modelType)
    {
        if (modelType == null) throw new ArgumentNullException(nameof(modelType));

        if (modelType == typeof(Utf8Memory)) return Utf8Memory.Empty;
        if (modelType == typeof(ReadOnlyUtf8Memory)) return ReadOnlyUtf8Memory.Empty;

        throw new NotSupportedException($"Type {modelType} not supported");
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