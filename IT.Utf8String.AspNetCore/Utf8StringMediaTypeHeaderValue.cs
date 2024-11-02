using Microsoft.Net.Http.Headers;

namespace IT.Net.Http.Headers;

public static class Utf8StringMediaTypeHeaderValue
{
    public static readonly MediaTypeHeaderValue TextPlainUtf8 = new("text/plain") { Charset = "utf-8" };
}