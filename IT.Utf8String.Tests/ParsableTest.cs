using IT;

namespace Tests;

public class ParsableTest
{
    [Test]
    public void Parse_Test()
    {
        var chars = "моя строка".ToArray();
        var bytes = "моя строка"u8.ToArray();

        Assert.That(Utf8Memory.Parse(chars), Is.EqualTo(new Utf8Memory(bytes)));
        Assert.That(Utf8Memory.TryParse(chars, out var utf8Memory), Is.True);
        Assert.That(utf8Memory, Is.EqualTo(new Utf8Memory(bytes)));

        Assert.That(Utf8Memory.Parse(chars.AsSpan().Slice(4, 3)), Is.EqualTo(new Utf8Memory("стр"u8.ToArray())));
    }

    [Test]
    public void RO_Parse_Test()
    {
        var chars = "моя строка".ToArray();
        var bytes = "моя строка"u8.ToArray();

        Assert.That(ReadOnlyUtf8Memory.Parse(chars), Is.EqualTo(new ReadOnlyUtf8Memory(bytes)));
        Assert.That(ReadOnlyUtf8Memory.TryParse(chars, out var utf8Memory), Is.True);
        Assert.That(utf8Memory, Is.EqualTo(new ReadOnlyUtf8Memory(bytes)));

        Assert.That(ReadOnlyUtf8Memory.Parse(chars.AsSpan().Slice(4, 3)), Is.EqualTo(new ReadOnlyUtf8Memory("стр"u8.ToArray())));
    }
}