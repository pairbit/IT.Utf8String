using System;
using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT;

[DebuggerDisplay("{ToString()}")]
[TypeConverter(typeof(Utf8StringTypeConverter))]
[JsonConverter(typeof(Utf8StringJsonConverter))]
public readonly struct Utf8String : IComparable<Utf8String>, IEquatable<Utf8String>, IFormattable
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
    private const int GB = 1073741824;

    class Utf8StringTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(string) ||
               sourceType == typeof(char[]) ||
               sourceType == typeof(byte[]) ||
               base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
        {
            if (value is string str) return Parse(str.AsSpan());
            if (value is char[] chars) return Parse(chars);
            if (value is byte[] bytes) return new Utf8String(bytes);

            return base.ConvertFrom(context, culture, value);
        }
    }

    class Utf8StringJsonConverter : JsonConverter<Utf8String>
    {
        public override Utf8String Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var tokenType = reader.TokenType;
            if (tokenType == JsonTokenType.Null) return default;
            if (tokenType != JsonTokenType.String) throw new JsonException("Expected string");

            if (reader.ValueIsEscaped)
            {
                int length = reader.GetLength(GB);
                if (length == 0) return default;

                var rented = ArrayPool<byte>.Shared.Rent(length);
                try
                {
                    var span = rented.AsSpan();
                    var written = reader.CopyString(span);
                    return new(span.Slice(0, written).ToArray());
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }
            else
            {
                int length = reader.GetLength();
                if (length == 0) return default;

                var bytes = new byte[length];

                var written = reader.CopyString(bytes);

                if (length != written) throw new JsonException("length != written");

                return new(bytes);
            }
        }

        public override void Write(Utf8JsonWriter writer, Utf8String value, JsonSerializerOptions options)
            => writer.WriteStringValue(value);
    }

    private readonly byte[]? _value;

    public static Utf8String Empty => default;

    public Memory<byte> Memory => _value;

    public Span<byte> Span => _value.AsSpan();

    public int Length => _value == null ? 0 : _value.Length;

    public bool IsEmpty => _value == null || _value.Length == 0;

    public Utf8String(byte[]? value)
    {
        _value = value;
    }

#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValid() => System.Text.Unicode.Utf8.IsValid(_value);
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Utf8Memory AsMemory(int start) => new(_value.AsMemory(start));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Utf8Memory AsMemory(int start, int length) => new(_value.AsMemory(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyUtf8Memory AsReadOnly() => new(_value);

    public int CompareTo(Utf8String other) => _value.AsSpan().SequenceCompareTo(other._value);

    public bool Equals(Utf8String other) => _value == other._value ||
        _value.AsSpan().SequenceEqual(other._value);

    public override bool Equals(object? obj)
        => obj is Utf8String utf8String && Equals(utf8String);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.AddBytes(_value);
        return hash.ToHashCode();
    }

    #region ToString

    public override string ToString() => _value == null || _value.Length == 0 ? string.Empty : Encoding.UTF8.GetString(_value);

    public char[] ToChars()
    {
        var count = Encoding.UTF8.GetCharCount(_value.AsSpan());
        if (count == 0) return [];

        var chars = new char[count];

#if NET6_0_OR_GREATER
        var status = System.Text.Unicode.Utf8.ToUtf16(_value, chars, out _, out _);
        if (status != System.Buffers.OperationStatus.Done)
            throw new InvalidOperationException($"OperationStatus is '{status}'");
#else
        Encoding.UTF8.GetChars(_value, chars);
#endif

        return chars;
    }

    public bool TryFormat(Span<char> chars, out int written)
    {
        if (_value == null || _value.Length == 0)
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
        var status = System.Text.Unicode.Utf8.ToUtf16(_value, chars, out _, out written);
        if (status != System.Buffers.OperationStatus.Done)
        {
            if (status == System.Buffers.OperationStatus.DestinationTooSmall) return false;
            throw new InvalidOperationException($"OperationStatus is '{status}'");
        }
#else
        written = Encoding.UTF8.GetChars(_value, chars);
#endif

        return true;
    }

    public bool TryFormat(Span<byte> bytes, out int written)
    {
        if (_value == null || _value.Length == 0)
        {
            written = 0;
            return true;
        }
        if (bytes.Length < _value.Length)
        {
            written = 0;
            return false;
        }

        _value.AsSpan().CopyTo(bytes);
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

        if (status != System.Buffers.OperationStatus.Done) throw new ArgumentException($"OperationStatus is '{status}'", nameof(chars));
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

    #region Operators

    public static bool operator ==(Utf8String left, Utf8String right) => left.Equals(right);

    public static bool operator !=(Utf8String left, Utf8String right) => !left.Equals(right);

    public static implicit operator Memory<byte>(Utf8String value) => value._value;

    public static implicit operator Span<byte>(Utf8String value) => value._value;

    public static implicit operator ReadOnlyMemory<byte>(Utf8String value) => value._value;

    public static implicit operator ReadOnlySpan<byte>(Utf8String value) => value._value;

    public static implicit operator Utf8Memory(Utf8String value) => new(value._value);

    public static implicit operator ReadOnlyUtf8Memory(Utf8String value) => new(value._value);

    public static implicit operator Utf8String(byte[] value) => new(value);

    #endregion Operators
}