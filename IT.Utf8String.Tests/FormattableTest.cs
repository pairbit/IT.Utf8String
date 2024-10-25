using IT;
using System.Text.Unicode;

namespace Tests;

public class FormattableTest
{
    [Test]
    public void ISpanFormattable_Test()
    {
        var utf8str = new Utf8String("моя строка"u8.ToArray());

        Assert.That($"Format '{utf8str}' str", Is.EqualTo("Format 'моя строка' str"));
    }

    [Test]
    public void IUtf8SpanFormattable_Test()
    {
        var utf8str = new Utf8String("моя строка"u8.ToArray());

        Span<byte> bytes = new byte[100];

        Assert.That(Utf8.TryWrite(bytes, $"Format '{utf8str}' str", out var written), Is.True);

        Assert.That(bytes[..written].SequenceEqual("Format 'моя строка' str"u8), Is.True);
    }
}