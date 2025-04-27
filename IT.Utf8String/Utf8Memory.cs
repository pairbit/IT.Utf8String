using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT;

[DebuggerDisplay("{ToString()}")]
[TypeConverter(typeof(Utf8MemoryTypeConverter))]
[JsonConverter(typeof(Utf8MemoryJsonConverter))]
public readonly struct Utf8Memory : IComparable<Utf8Memory>, IEquatable<Utf8Memory>, IFormattable
#if NET6_0_OR_GREATER
, ISpanFormattable
#endif
#if NET7_0_OR_GREATER
, ISpanParsable<Utf8Memory>
#endif
#if NET8_0_OR_GREATER
, IUtf8SpanFormattable, IUtf8SpanParsable<Utf8Memory>
#endif
{
    class Utf8MemoryTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(Utf8Memory) ||
               sourceType == typeof(ReadOnlyUtf8Memory) ||
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
            if (value is Utf8Memory utf8Memory) return utf8Memory;
            if (value is ReadOnlyUtf8Memory readOnlyUtf8Memory) return new Utf8Memory(readOnlyUtf8Memory.Memory.ToArray());
            if (value is string str) return new Utf8Memory(Parse(str.AsSpan()));
            if (value is char[] chars) return new Utf8Memory(Parse(chars));
            if (value is Memory<char> memoryChar) return new Utf8Memory(Parse(memoryChar.Span));
            if (value is ReadOnlyMemory<char> readOnlyMemoryChar) return new Utf8Memory(Parse(readOnlyMemoryChar.Span));
            if (value is byte[] bytes) return new Utf8Memory(bytes);
            if (value is Memory<byte> memoryByte) return new Utf8Memory(memoryByte);
            if (value is ReadOnlyMemory<byte> readOnlyMemoryByte) return new Utf8Memory(readOnlyMemoryByte.ToArray());

            return base.ConvertFrom(context, culture, value);
        }
    }

    class Utf8MemoryJsonConverter : JsonConverter<Utf8Memory>
    {
        public override Utf8Memory Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

        public override void Write(Utf8JsonWriter writer, Utf8Memory value, JsonSerializerOptions options)
            => writer.WriteStringValue(value);
    }

    private readonly Memory<byte> _value;

    public static Utf8Memory Empty => default;

    public Memory<byte> Memory => _value;

    public Span<byte> Span => _value.Span;

    public int Length => _value.Length;

    public bool IsEmpty => _value.Length == 0;

    public Utf8Memory(Memory<byte> value)
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
    public Utf8Memory Slice(int start) => new(_value.Slice(start));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Utf8Memory Slice(int start, int length) => new(_value.Slice(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyUtf8Memory AsReadOnly() => new(_value);

    public int CompareTo(Utf8Memory other) => _value.Span.SequenceCompareTo(other._value.Span);

    public bool Equals(Utf8Memory other) => _value.Equals(other._value) ||
        _value.Span.SequenceEqual(other._value.Span);

    public override bool Equals(object? obj)
        => obj is Utf8Memory utf8Memory && Equals(utf8Memory);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.AddBytes(_value.Span);
        return hash.ToHashCode();
    }

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

    public static Utf8Memory Parse(ReadOnlySpan<byte> bytes) => new(bytes.ToArray());

    public static bool TryParse(ReadOnlySpan<byte> bytes, out Utf8Memory utf8Memory)
    {
        utf8Memory = new(bytes.ToArray());
        return true;
    }

    /// <exception cref="ArgumentException"></exception>
    public static Utf8Memory Parse(ReadOnlySpan<char> chars)
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

    public static bool TryParse(ReadOnlySpan<char> chars, out Utf8Memory utf8Memory)
    {
        var count = Encoding.UTF8.GetByteCount(chars);
        if (count == 0)
        {
            utf8Memory = default;
            return true;
        }

        var bytes = new byte[count];
#if NET6_0_OR_GREATER
        var status = System.Text.Unicode.Utf8.FromUtf16(chars, bytes, out _, out _);

        if (status != System.Buffers.OperationStatus.Done)
        {
            utf8Memory = default;
            return false;
        }
#else
        Encoding.UTF8.GetBytes(chars, bytes);
#endif
        utf8Memory = new(bytes);
        return true;
    }

    #endregion Parse

    #region Parsable

#if NET8_0_OR_GREATER

    static Utf8Memory IUtf8SpanParsable<Utf8Memory>.Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)
        => Parse(utf8Text);

    static bool IUtf8SpanParsable<Utf8Memory>.TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out Utf8Memory result)
        => TryParse(utf8Text, out result);

#endif

#if NET7_0_OR_GREATER

    static Utf8Memory ISpanParsable<Utf8Memory>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => Parse(s);

    static bool ISpanParsable<Utf8Memory>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out Utf8Memory result)
        => TryParse(s, out result);

    static Utf8Memory IParsable<Utf8Memory>.Parse(string s, IFormatProvider? provider)
        => Parse(s);

    static bool IParsable<Utf8Memory>.TryParse([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string? s, IFormatProvider? provider, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out Utf8Memory result)
        => TryParse(s, out result);

#endif

    #endregion Parsable

    #region Operators

    public static bool operator ==(Utf8Memory left, Utf8Memory right) => left.Equals(right);

    public static bool operator !=(Utf8Memory left, Utf8Memory right) => !left.Equals(right);

    public static implicit operator Memory<byte>(Utf8Memory value) => value._value;

    public static implicit operator Span<byte>(Utf8Memory value) => value._value.Span;

    public static implicit operator ReadOnlyMemory<byte>(Utf8Memory value) => value._value;

    public static implicit operator ReadOnlySpan<byte>(Utf8Memory value) => value._value.Span;

    public static implicit operator ReadOnlyUtf8Memory(Utf8Memory value) => new(value._value);

    public static implicit operator Utf8Memory(Memory<byte> value) => new(value);

    public static implicit operator Utf8Memory(byte[] value) => new(value);

    public static implicit operator Utf8Memory(ArraySegment<byte> value) => new(value);

    #endregion Operators
}