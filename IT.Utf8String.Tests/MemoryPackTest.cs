#if NET6_0_OR_GREATER

using IT;
using MemoryPack;
using MemoryPack.Formatters;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Tests;

[MemoryPackable]
public partial record Strs
{
    static partial void StaticConstructor()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<Utf8String>())
        {
            MemoryPackFormatterProvider.Register(Utf8StringMemoryPackFormatter.Default);
        }
        if (!MemoryPackFormatterProvider.IsRegistered<ReadOnlyUtf8String>())
        {
            MemoryPackFormatterProvider.Register(ReadOnlyUtf8StringMemoryPackFormatter.Default);
        }
    }

    public string Str { get; set; } = null!;

    [MemoryPackAllowSerialize]
    public Utf8String Utf8Str { get; set; }

    [MemoryPackAllowSerialize]
    public ReadOnlyUtf8String ROUtf8Str { get; set; }
}

[MemoryPackable]
public partial class StrsPool : IDisposable, IEquatable<StrsPool>
{
    private bool _usePool;

    public string Str { get; set; } = null!;

    [MemoryPackAllowSerialize]
    [Utf8StringPoolFormatter]
    public Utf8String Utf8Str { get; set; }

    [MemoryPackAllowSerialize]
    [ReadOnlyUtf8StringPoolFormatter]
    public ReadOnlyUtf8String ROUtf8Str { get; set; }

    public void Dispose()
    {
        if (!_usePool) return;

        Return(Utf8Str);
        Utf8Str = default;

        Return(ROUtf8Str);
        ROUtf8Str = default;
    }

    [MemoryPackOnDeserialized]
    void OnDeserialized()
    {
        _usePool = true;
    }

    static void Return(ReadOnlyMemory<byte> memory)
    {
        if (MemoryMarshal.TryGetArray(memory, out var segment) && segment.Array is { Length: > 0 })
        {
            ArrayPool<byte>.Shared.Return(segment.Array);
        }
    }

    public bool Equals(StrsPool? other) => other != null && 
        Str.Equals(other.Str) &&
        Utf8Str.Equals(other.Utf8Str) &&
        ROUtf8Str.Equals(other.ROUtf8Str);

    public override bool Equals(object? obj)
        => obj is StrsPool strs && Equals(strs);

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}

public class MemoryPackTest
{
    [Test]
    public void BaseTest()
    {
        var str = "string";
        var utf8Str = new Utf8String("string"u8.ToArray());

        var str16Bytes = MemoryPackSerializer.Serialize(str, MemoryPackSerializerOptions.Utf16);
        Assert.That(str16Bytes.Length, Is.EqualTo(16));

        var strBytes = MemoryPackSerializer.Serialize(str, MemoryPackSerializerOptions.Utf8);
        Assert.That(strBytes.Length, Is.EqualTo(14));

        MemoryPackFormatterProvider.Register(Utf8StringMemoryPackFormatter.Default);
        var utf8StrBytes = MemoryPackSerializer.Serialize(utf8Str);
        Assert.That(utf8StrBytes.Length, Is.EqualTo(10));

        Assert.That(strBytes[4..].SequenceEqual(utf8StrBytes), Is.True);
    }

    [Test]
    public void StrsTest()
    {
        var s = new Strs
        {
            Str = "Str",
            Utf8Str = "Utf8Str"u8.ToArray(),
            ROUtf8Str = "ROUtf8Str"u8.ToArray()
        };

        var bytes = MemoryPackSerializer.Serialize(s);

        var s2 = MemoryPackSerializer.Deserialize<Strs>(bytes);

        Assert.That(s, Is.EqualTo(s2));
    }

    [Test]
    public void StrsPoolTest()
    {
        using var s = new StrsPool
        {
            Str = "StrPool",
            Utf8Str = "Utf8StrPool"u8.ToArray(),
            ROUtf8Str = "ROUtf8StrPool"u8.ToArray()
        };

        var bytes = MemoryPackSerializer.Serialize(s);

        using var s2 = MemoryPackSerializer.Deserialize<StrsPool>(bytes);

        Assert.That(s, Is.EqualTo(s2));
    }
}

#endif