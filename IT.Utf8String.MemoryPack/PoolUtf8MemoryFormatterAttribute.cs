using IT;
using MemoryPack.Formatters;

namespace MemoryPack;

public sealed class PoolUtf8MemoryFormatterAttribute : MemoryPackCustomFormatterAttribute<PoolUtf8MemoryPackFormatter, Utf8Memory>
{
    public override PoolUtf8MemoryPackFormatter GetFormatter() =>
        PoolUtf8MemoryPackFormatter.Default;
}