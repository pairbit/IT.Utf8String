using IT;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Formatters;

public sealed class ReadOnlyUtf8StringPoolMemoryPackFormatter : MemoryPackFormatter<ReadOnlyUtf8String>
{
    public static readonly ReadOnlyUtf8StringPoolMemoryPackFormatter Default = new();

    private readonly ArrayPool<byte>? _pool;

    public ReadOnlyUtf8StringPoolMemoryPackFormatter()
    {
        _pool = null;
    }

    public ReadOnlyUtf8StringPoolMemoryPackFormatter(ArrayPool<byte> pool)
    {
        _pool = pool;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ReadOnlyUtf8String value)
    {
        writer.WriteUnmanagedSpan(value.Span);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ReadOnlyUtf8String value)
    {
        if (!reader.TryReadCollectionHeader(out var length) || length == 0)
        {
            value = default;
            return;
        }

        var pool = _pool ?? ArrayPool<byte>.Shared;

        var memory = pool.Rent(length).AsMemory(0, length);

        Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(memory.Span), ref reader.GetSpanReference(length), (uint)length);

        reader.Advance(length);

        value = new(memory);
    }
}