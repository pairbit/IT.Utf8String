using IT;
using MemoryPack.Formatters;

namespace MemoryPack;

public sealed class Utf8StringPoolFormatterAttribute : MemoryPackCustomFormatterAttribute<Utf8StringPoolMemoryPackFormatter, Utf8String>
{
    public override Utf8StringPoolMemoryPackFormatter GetFormatter() =>
        Utf8StringPoolMemoryPackFormatter.Default;
}