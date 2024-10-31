using IT;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Formatters;

public sealed class Utf8StringPoolMemoryPackFormatter : MemoryPackFormatter<Utf8String>
{
    public static readonly Utf8StringPoolMemoryPackFormatter Default = new();

    private readonly ArrayPool<byte>? _pool;

    public Utf8StringPoolMemoryPackFormatter()
    {
        _pool = null;
    }

    public Utf8StringPoolMemoryPackFormatter(ArrayPool<byte> pool)
    {
        _pool = pool;
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Utf8String value)
    {
        writer.WriteUnmanagedSpan(value.Span);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Utf8String value)
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