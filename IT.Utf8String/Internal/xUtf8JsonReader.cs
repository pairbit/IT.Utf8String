using System.Text.Json;

namespace IT;

internal static class xUtf8JsonReader
{
    public const int Max = 2147483591;

    public static int GetLength(this ref Utf8JsonReader reader, int maxLength = Max)
    {
        if (reader.HasValueSequence)
        {
            var longLength = reader.ValueSequence.Length;
            if (longLength == 0) return default;
            if (longLength > maxLength) throw new JsonException("string too long");
            return checked((int)longLength);
        }
        else
        {
            var length = reader.ValueSpan.Length;
            if (length > maxLength) throw new JsonException("string too long");
            return length;
        }
    }
}