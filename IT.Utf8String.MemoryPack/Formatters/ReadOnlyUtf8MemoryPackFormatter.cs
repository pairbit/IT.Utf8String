using IT;

namespace MemoryPack.Formatters;

public sealed class ReadOnlyUtf8MemoryPackFormatter : MemoryPackFormatter<ReadOnlyUtf8Memory>
{
    public static readonly ReadOnlyUtf8MemoryPackFormatter Default = new();

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ReadOnlyUtf8Memory value)
    {
        writer.WriteUnmanagedSpan(value.Span);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ReadOnlyUtf8Memory value)
    {
        var array = reader.ReadUnmanagedArray<byte>();
        value = array == null ? default : new(array);
    }
}