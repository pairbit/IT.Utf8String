using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT;

[DebuggerDisplay("{ToString()}")]
[TypeConverter(typeof(ReadOnlyUtf8StringTypeConverter))]
[JsonConverter(typeof(ReadOnlyUtf8StringJsonConverter))]
public readonly struct ReadOnlyUtf8String : IEquatable<ReadOnlyUtf8String>, IFormattable
#if NET6_0_OR_GREATER
, ISpanFormattable
#endif
#if NET7_0_OR_GREATER
, ISpanParsable<ReadOnlyUtf8String>
#endif
#if NET8_0_OR_GREATER
, IUtf8SpanFormattable, IUtf8SpanParsable<ReadOnlyUtf8String>
#endif
{
    class ReadOnlyUtf8StringTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(ReadOnlyUtf8String) ||
               sourceType == typeof(Utf8String) ||
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
            if (value is ReadOnlyUtf8String readOnlyUtf8String) return readOnlyUtf8String;
            if (value is Utf8String utf8String) return utf8String.AsReadOnly();
            if (value is string str) return new ReadOnlyUtf8String(Parse(str.AsSpan()));
            if (value is char[] chars) return new ReadOnlyUtf8String(Parse(chars));
            if (value is Memory<char> memoryChar) return new ReadOnlyUtf8String(Parse(memoryChar.Span));
            if (value is ReadOnlyMemory<char> readOnlyMemoryChar) return new ReadOnlyUtf8String(Parse(readOnlyMemoryChar.Span));
            if (value is byte[] bytes) return new ReadOnlyUtf8String(bytes);
            if (value is Memory<byte> memoryByte) return new ReadOnlyUtf8String(memoryByte);
            if (value is ReadOnlyMemory<byte> readOnlyMemoryByte) return new ReadOnlyUtf8String(readOnlyMemoryByte.ToArray());

            return base.ConvertFrom(context, culture, value);
        }
    }

    class ReadOnlyUtf8StringJsonConverter : JsonConverter<ReadOnlyUtf8String>
    {
        public override ReadOnlyUtf8String Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var tokenType = reader.TokenType;
            if (tokenType == JsonTokenType.Null) return default;
            if (tokenType != JsonTokenType.String) throw new JsonException("Expected string");

            var length = reader.HasValueSequence ? reader.ValueSequence.Length : reader.ValueSpan.Length;
            if (length == 0) return default;

            var bytes = new byte[length];

            var written = reader.CopyString(bytes);

            return bytes.AsMemory(0, written);
        }

        public override void Write(Utf8JsonWriter writer, ReadOnlyUtf8String value, JsonSerializerOptions options)
            => writer.WriteStringValue(value);
    }

    private readonly ReadOnlyMemory<byte> _value;

    public static ReadOnlyUtf8String Empty => default;

    public ReadOnlyMemory<byte> Memory => _value;

    public ReadOnlySpan<byte> Span => _value.Span;

    public int Length => _value.Length;

    public bool IsEmpty => _value.Length == 0;

    public ReadOnlyUtf8String(ReadOnlyMemory<byte> value)
    {
        _value = value;
    }

#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValid() => System.Text.Unicode.Utf8.IsValid(_value.Span);
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetArray(out ArraySegment<byte> segment)
        => System.Runtime.InteropServices.MemoryMarshal.TryGetArray(_value, out segment);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyUtf8String Slice(int start) => new(_value.Slice(start));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyUtf8String Slice(int start, int length) => new(_value.Slice(start, length));

    public bool Equals(ReadOnlyUtf8String other) => _value.Equals(other._value) ||
        _value.Span.SequenceEqual(other._value.Span);

    public override bool Equals(object? obj)
        => obj is ReadOnlyUtf8String utf8String && Equals(utf8String);

    public override int GetHashCode() => _value.GetHashCode();

    #region ToString

    public override string ToString() => _value.Length == 0 ? string.Empty : Encoding.UTF8.GetString(_value.Span);

    public char[] ToChars()
    {
        var count = Encoding.UTF8.GetCharCount(_value.Span);
        if (count == 0) return [];

        var chars = new char[count];

#if NET6_0_OR_GREATER
        var status = System.Text.Unicode.Utf8.ToUtf16(_value.Span, chars, out _, out _);
        if (status != System.Buffers.OperationStatus.Done)
            throw new InvalidOperationException($"OperationStatus is '{status}'");
#else
        Encoding.UTF8.GetChars(_value.Span, chars);
#endif

        return chars;
    }

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
        if (status != System.Buffers.OperationStatus.Done)
        {
            if (status == System.Buffers.OperationStatus.DestinationTooSmall) return false;
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

    #endregion ToString

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

    #region Parse

    public static ReadOnlyUtf8String Parse(ReadOnlySpan<byte> bytes) => new(bytes.ToArray());

    public static bool TryParse(ReadOnlySpan<byte> bytes, out ReadOnlyUtf8String utf8String)
    {
        utf8String = new(bytes.ToArray());
        return true;
    }

    /// <exception cref="ArgumentException"></exception>
    public static ReadOnlyUtf8String Parse(ReadOnlySpan<char> chars)
    {
        var count = Encoding.UTF8.GetByteCount(chars);
        if (count == 0) return default;

        var bytes = new byte[count];
#if NET6_0_OR_GREATER
        var status = System.Text.Unicode.Utf8.FromUtf16(chars, bytes, out _, out _);

        if (status != System.Buffers.OperationStatus.Done) throw new ArgumentException($"OperationStatus is '{status}'", nameof(chars));
#else
        Encoding.UTF8.GetBytes(chars, bytes);
#endif
        return new(bytes);
    }

    public static bool TryParse(ReadOnlySpan<char> chars, out ReadOnlyUtf8String utf8String)
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

        if (status != System.Buffers.OperationStatus.Done)
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

    #endregion Parse

    #region Parsable

#if NET8_0_OR_GREATER

    static ReadOnlyUtf8String IUtf8SpanParsable<ReadOnlyUtf8String>.Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)
        => Parse(utf8Text);

    static bool IUtf8SpanParsable<ReadOnlyUtf8String>.TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out ReadOnlyUtf8String result)
        => TryParse(utf8Text, out result);

#endif

#if NET7_0_OR_GREATER

    static ReadOnlyUtf8String ISpanParsable<ReadOnlyUtf8String>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => Parse(s);

    static bool ISpanParsable<ReadOnlyUtf8String>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out ReadOnlyUtf8String result)
        => TryParse(s, out result);

    static ReadOnlyUtf8String IParsable<ReadOnlyUtf8String>.Parse(string s, IFormatProvider? provider)
        => Parse(s);

    static bool IParsable<ReadOnlyUtf8String>.TryParse([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string? s, IFormatProvider? provider, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out ReadOnlyUtf8String result)
        => TryParse(s, out result);

#endif

    #endregion Parsable

    #region Operators

    public static bool operator ==(ReadOnlyUtf8String left, ReadOnlyUtf8String right) => left.Equals(right);

    public static bool operator !=(ReadOnlyUtf8String left, ReadOnlyUtf8String right) => !left.Equals(right);

    public static implicit operator ReadOnlyMemory<byte>(ReadOnlyUtf8String value) => value._value;

    public static implicit operator ReadOnlySpan<byte>(ReadOnlyUtf8String value) => value._value.Span;

    public static implicit operator ReadOnlyUtf8String(Memory<byte> value) => new(value);

    public static implicit operator ReadOnlyUtf8String(ReadOnlyMemory<byte> value) => new(value);

    public static implicit operator ReadOnlyUtf8String(byte[] value) => new(value);

    public static implicit operator ReadOnlyUtf8String(ArraySegment<byte> value) => new(value);

    #endregion Operators
}