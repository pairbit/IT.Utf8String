using Microsoft.Net.Http.Headers;

namespace IT.AspNetCore.Mvc.Formatters;

internal static class Utf8StringMediaType
{
    public static readonly MediaTypeHeaderValue TextPlainUtf8 = new("text/plain") { Charset = "utf-8" };
}