#if NET6_0_OR_GREATER

using IT;
using MemoryPack;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Tests;

[MemoryPackable]
public partial class Strs : IDisposable, IEquatable<Strs>
{
    private bool _usePool;

    static partial void StaticConstructor()
    {
        //if (!MemoryPackFormatterProvider.IsRegistered<Utf8String>())
        //{
        //    MemoryPackFormatterProvider.Register(Utf8StringMemoryPackFormatter.Default);
        //}
        //if (!MemoryPackFormatterProvider.IsRegistered<ReadOnlyUtf8String>())
        //{
        //    MemoryPackFormatterProvider.Register(ReadOnlyUtf8StringMemoryPackFormatter.Default);
        //}
    }

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

    public bool Equals(Strs? other) => other != null && 
        Str.Equals(other.Str) &&
        Utf8Str.Equals(other.Utf8Str) &&
        ROUtf8Str.Equals(other.ROUtf8Str);

    public override bool Equals(object? obj)
        => obj is Strs strs && Equals(strs);

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}

public class MemoryPackTest
{
    [Test]
    public void PackTest()
    {
        using var s = new Strs
        {
            Str = "Str",
            Utf8Str = "Utf8Str"u8.ToArray(),
            ROUtf8Str = "ROUtf8Str"u8.ToArray()
        };

        var bytes = MemoryPackSerializer.Serialize(s);

        using var s2 = MemoryPackSerializer.Deserialize<Strs>(bytes);

        Assert.That(s, Is.EqualTo(s2));
    }
}

#endif