using IT;

namespace MemoryPack.Formatters;

public sealed class ReadOnlyUtf8StringMemoryPackFormatter : MemoryPackFormatter<ReadOnlyUtf8String>
{
    public static readonly ReadOnlyUtf8StringMemoryPackFormatter Default = new();

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ReadOnlyUtf8String value)
    {
        writer.WriteUnmanagedSpan(value.Span);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ReadOnlyUtf8String value)
    {
        var array = reader.ReadUnmanagedArray<byte>();
        value = array == null ? default : new(array);
    }
}