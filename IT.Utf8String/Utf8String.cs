using System;
using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT;

[DebuggerDisplay("{ToString()}")]
[TypeConverter(typeof(Utf8StringTypeConverter))]
[JsonConverter(typeof(Utf8StringJsonConverter))]
public readonly struct Utf8String : IEquatable<Utf8String>, IFormattable
#if NET6_0_OR_GREATER
, ISpanFormattable
#endif
#if NET7_0_OR_GREATER
, ISpanParsable<Utf8String>
#endif
#if NET8_0_OR_GREATER
, IUtf8SpanFormattable, IUtf8SpanParsable<Utf8String>
#endif
{
    class Utf8StringTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(Utf8String) ||
               sourceType == typeof(string) ||
               sourceType == typeof(char[]) ||
               sourceType == typeof(Memory<char>) ||
               sourceType == typeof(ReadOnlyMemory<char>) ||
               sourceType == typeof(byte[]) ||
               sourceType == typeof(Memory<byte>) ||
               sourceType == typeof(ReadOnlyMemory<byte>) ||
               base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
        {
            if (value is Utf8String utf8String) return utf8String;
            if (value is string str) return new Utf8String(Parse(str.AsSpan()));
            if (value is char[] chars) return new Utf8String(Parse(chars));
            if (value is Memory<char> memoryChar) return new Utf8String(Parse(memoryChar.Span));
            if (value is ReadOnlyMemory<char> readOnlyMemoryChar) return new Utf8String(Parse(readOnlyMemoryChar.Span));
            if (value is byte[] bytes) return new Utf8String(bytes);
            if (value is Memory<byte> memoryByte) return new Utf8String(memoryByte);
            if (value is ReadOnlyMemory<byte> readOnlyMemoryByte) return new Utf8String(readOnlyMemoryByte.ToArray());

            return base.ConvertFrom(context, culture, value);
        }
    }

    class Utf8StringJsonConverter : JsonConverter<Utf8String>
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

    private readonly Memory<byte> _value;

    public static Utf8String Empty => default;

    public Memory<byte> Memory => _value;

    public Span<byte> Span => _value.Span;

    public int Length => _value.Length;

    public bool IsEmpty => _value.Length == 0;

    public Utf8String(Memory<byte> value)
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
        if (_value.Length == 0)
        {
            written = 0;
            return true;
        }
        //TODO: chars length != bytes length
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
        if (_value.Length == 0)
        {
            written = 0;
            return true;
        }
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

    public override bool Equals(object? obj)
        => obj is Utf8String utf8String && Equals(utf8String);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.Length == 0 ? string.Empty : Encoding.UTF8.GetString(_value.Span);

    public char[] ToChars()
    {
        var count = Encoding.UTF8.GetCharCount(_value.Span);
        if (count == 0) return [];

        var chars = new char[count];

#if NET6_0_OR_GREATER
        var status = System.Text.Unicode.Utf8.ToUtf16(_value.Span, chars, out _, out _);
        if (status != OperationStatus.Done)
            throw new InvalidOperationException($"OperationStatus is '{status}'");
#else
        Encoding.UTF8.GetChars(_value.Span, chars);
#endif

        return chars;
    }

    public bool TryGetArray(out ArraySegment<byte> segment)
        => System.Runtime.InteropServices.MemoryMarshal.TryGetArray(_value, out segment);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Utf8String Slice(int start) => new(Memory.Slice(start));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Utf8String Slice(int start, int length) => new(Memory.Slice(start, length));

    public static bool operator ==(Utf8String left, Utf8String right) => left.Equals(right);

    public static bool operator !=(Utf8String left, Utf8String right) => !left.Equals(right);

    public static implicit operator Memory<byte>(Utf8String value) => value._value;

    public static implicit operator Span<byte>(Utf8String value) => value._value.Span;

    public static implicit operator ReadOnlyMemory<byte>(Utf8String value) => value._value;

    public static implicit operator ReadOnlySpan<byte>(Utf8String value) => value._value.Span;

    public static implicit operator Utf8String(Memory<byte> value) => new(value);

    public static implicit operator Utf8String(byte[] value) => new(value);

    public static Utf8String Parse(ReadOnlySpan<byte> bytes) => new(bytes.ToArray());

    public static bool TryParse(ReadOnlySpan<byte> bytes, out Utf8String utf8String)
    {
        utf8String = new(bytes.ToArray());
        return true;
    }

    /// <exception cref="ArgumentException"></exception>
    public static Utf8String Parse(ReadOnlySpan<char> chars)
    {
        var count = Encoding.UTF8.GetByteCount(chars);
        if (count == 0) return default;

        var bytes = new byte[count];
#if NET6_0_OR_GREATER
        var status = System.Text.Unicode.Utf8.FromUtf16(chars, bytes, out _, out _);

        if (status != OperationStatus.Done) throw new ArgumentException($"OperationStatus is '{status}'", nameof(chars));
#else
        Encoding.UTF8.GetBytes(chars, bytes);
#endif
        return new(bytes);
    }

    public static bool TryParse(ReadOnlySpan<char> chars, out Utf8String utf8String)
    {
        var count = Encoding.UTF8.GetByteCount(chars);
        if (count == 0)
        {
            utf8String = default;
            return true;
        }

        var bytes = new byte[count];
#if NET6_0_OR_GREATER
        var status = System.Text.Unicode.Utf8.FromUtf16(chars, bytes, out _, out _);

        if (status != OperationStatus.Done)
        {
            utf8String = default;
            return false;
        }
#else
        Encoding.UTF8.GetBytes(chars, bytes);
#endif
        utf8String = new(bytes);
        return true;
    }

    #region Parsable

#if NET8_0_OR_GREATER

    static Utf8String IUtf8SpanParsable<Utf8String>.Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)
        => Parse(utf8Text);

    static bool IUtf8SpanParsable<Utf8String>.TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out Utf8String result)
        => TryParse(utf8Text, out result);

#endif

#if NET7_0_OR_GREATER

    static Utf8String ISpanParsable<Utf8String>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => Parse(s);

    static bool ISpanParsable<Utf8String>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out Utf8String result)
        => TryParse(s, out result);

    static Utf8String IParsable<Utf8String>.Parse(string s, IFormatProvider? provider)
        => Parse(s);

    static bool IParsable<Utf8String>.TryParse([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string? s, IFormatProvider? provider, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out Utf8String result)
        => TryParse(s, out result);

#endif

    #endregion Parsable
}