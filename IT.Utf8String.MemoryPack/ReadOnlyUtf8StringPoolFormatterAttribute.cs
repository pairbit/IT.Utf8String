using IT;
using MemoryPack.Formatters;

namespace MemoryPack;

public sealed class ReadOnlyUtf8StringPoolFormatterAttribute : MemoryPackCustomFormatterAttribute<ReadOnlyUtf8StringPoolMemoryPackFormatter, ReadOnlyUtf8String>
{
    public override ReadOnlyUtf8StringPoolMemoryPackFormatter GetFormatter()
        => ReadOnlyUtf8StringPoolMemoryPackFormatter.Default;
}