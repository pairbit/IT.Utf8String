using MemoryPack;

namespace IT.MemoryPack.Formatters;

public sealed class Utf8StringMemoryPackFormatter : MemoryPackFormatter<Utf8String>
{
    public static readonly Utf8StringMemoryPackFormatter Default = new();

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Utf8String value)
    {
        writer.WriteUnmanagedSpan(value.Span);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Utf8String value)
    {
        var array = reader.ReadUnmanagedArray<byte>();
        value = array == null ? default : new(array);
    }
}