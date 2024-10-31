#if NET6_0_OR_GREATER

using IT;
using MemoryPack;
using MemoryPack.Formatters;

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

public class MemoryPackTest
{
    [Test]
    public void PackTest()
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
}

#endif