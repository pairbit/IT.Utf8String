using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT;

[DebuggerDisplay("{ToString(),nq}")]
[JsonConverter(typeof(JsonConverter))]
public readonly struct Utf8String : IEquatable<Utf8String>, IFormattable
#if NET6_0_OR_GREATER
, ISpanFormattable
#endif
#if NET8_0_OR_GREATER
, IUtf8SpanFormattable
#endif
{
    public class JsonConverter : JsonConverter<Utf8String>
    {
        public override Utf8String Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String) throw new JsonException("Expected string");

            if (reader.HasValueSequence)
            {
                var sequence = reader.ValueSequence;
                return sequence.IsSingleSegment ? sequence.First.Span.ToArray() : sequence.ToArray();
            }
            else
            {
                return reader.ValueSpan.ToArray();
            }
        }

        public override void Write(Utf8JsonWriter writer, Utf8String value, JsonSerializerOptions options)
            => writer.WriteStringValue(value);
    }

    private readonly ReadOnlyMemory<byte> _value;

    public ReadOnlyMemory<byte> Memory => _value;

    public ReadOnlySpan<byte> Span => _value.Span;

    public Utf8String(ReadOnlyMemory<byte> value)
    {
        _value = value;
    }

    #region Formattable

#if NET8_0_OR_GREATER
    bool IUtf8SpanFormattable.TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (format.Length != 0) throw new FormatException();

        return TryFormat(utf8Destination, out bytesWritten);
    }
#endif

#if NET6_0_OR_GREATER
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (format.Length != 0) throw new FormatException();

        return TryFormat(destination, out charsWritten);
    }
#endif

    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
    {
        if (format != null) throw new FormatException();

        return ToString();
    }

    #endregion Formattable

    public bool TryFormat(Span<char> chars, out int written)
    {
        if (chars.Length < _value.Length)
        {
            written = 0;
            return false;
        }

#if NET6_0_OR_GREATER
        var status = System.Text.Unicode.Utf8.ToUtf16(_value.Span, chars, out _, out written);
        if (status != OperationStatus.Done)
        {
            if (status == OperationStatus.DestinationTooSmall) return false;
            throw new InvalidOperationException($"OperationStatus is '{status}'");
        }
#else
        written = Encoding.UTF8.GetChars(_value.Span, chars);
#endif

        return true;
    }

    public bool TryFormat(Span<byte> bytes, out int written)
    {
        if (bytes.Length < _value.Length)
        {
            written = 0;
            return false;
        }

        _value.Span.CopyTo(bytes);
        written = _value.Length;
        return true;
    }

    public bool Equals(Utf8String other) => _value.Equals(other._value) ||
        _value.Span.SequenceEqual(other._value.Span);

    public override string ToString()
#if NETSTANDARD2_0
    {
        if (System.Runtime.InteropServices.MemoryMarshal.TryGetArray(_value, out var segment))
            return Encoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count);
        else
            return Encoding.UTF8.GetString(_value.Span);
    }
#else
        => Encoding.UTF8.GetString(_value.Span);
#endif

    public override bool Equals(object? obj)
        => obj is Utf8String utf8String && Equals(utf8String);

    public override int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(Utf8String left, Utf8String right) => left.Equals(right);

    public static bool operator !=(Utf8String left, Utf8String right) => !left.Equals(right);

    public static implicit operator ReadOnlyMemory<byte>(Utf8String value) => value._value;

    public static implicit operator ReadOnlySpan<byte>(Utf8String value) => value._value.Span;

    public static implicit operator Utf8String(ReadOnlyMemory<byte> value) => new(value);

    public static implicit operator Utf8String(byte[] value) => new(value);
}