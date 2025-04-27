using IT;
using MemoryPack.Formatters;

namespace MemoryPack;

public sealed class PoolReadOnlyUtf8MemoryFormatterAttribute : MemoryPackCustomFormatterAttribute<PoolReadOnlyUtf8MemoryPackFormatter, ReadOnlyUtf8Memory>
{
    public override PoolReadOnlyUtf8MemoryPackFormatter GetFormatter()
        => PoolReadOnlyUtf8MemoryPackFormatter.Default;
}