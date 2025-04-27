using IT;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Formatters;

public sealed class PoolReadOnlyUtf8MemoryPackFormatter : MemoryPackFormatter<ReadOnlyUtf8Memory>
{
    public static readonly PoolReadOnlyUtf8MemoryPackFormatter Default = new();

    private readonly ArrayPool<byte>? _pool;

    public PoolReadOnlyUtf8MemoryPackFormatter()
    {
        _pool = null;
    }

    public PoolReadOnlyUtf8MemoryPackFormatter(ArrayPool<byte> pool)
    {
        _pool = pool;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ReadOnlyUtf8Memory value)
    {
        writer.WriteUnmanagedSpan(value.Span);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ReadOnlyUtf8Memory value)
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