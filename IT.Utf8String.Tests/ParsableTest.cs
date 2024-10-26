using IT;

namespace Tests;

public class ParsableTest
{
    [Test]
    public void Parse_Test()
    {
        var chars = "моя строка".ToArray();
        var bytes = "моя строка"u8.ToArray();

        Assert.That(Utf8String.Parse(chars), Is.EqualTo(new Utf8String(bytes)));
        Assert.That(Utf8String.TryParse(chars, out var utf8String), Is.True);
        Assert.That(utf8String, Is.EqualTo(new Utf8String(bytes)));

        Assert.That(Utf8String.Parse(chars.AsSpan().Slice(4, 3)), Is.EqualTo(new Utf8String("стр"u8.ToArray())));
    }
}