using IT;

namespace MemoryPack.Formatters;

public sealed class Utf8MemoryPackFormatter : MemoryPackFormatter<Utf8Memory>
{
    public static readonly Utf8MemoryPackFormatter Default = new();

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Utf8Memory value)
    {
        writer.WriteUnmanagedSpan(value.Span);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Utf8Memory value)
    {
        var array = reader.ReadUnmanagedArray<byte>();
        value = array == null ? default : new(array);
    }
}