using IT;

namespace Tests;

public class FormattableTest
{
    [Test]
    public void TryFormat_Test()
    {
        var utf8String = new Utf8String("моя строка"u8.ToArray());
        Span<char> buffer = stackalloc char[100];

        Assert.That(utf8String.TryFormat(buffer, out var written), Is.True);
        Assert.That(buffer.Slice(0, written).SequenceEqual("моя строка".ToCharArray()), Is.True);

        var start = written;
        var str = "моя часть строки"u8.ToArray();
        var sliced = str.AsMemory().Slice(7, 10);
        utf8String = new Utf8String(sliced);
        Assert.That(utf8String.ToString(), Is.EqualTo("часть"));
        Assert.That(utf8String.ToChars(), Is.EqualTo("часть".ToCharArray()));

        Assert.That(utf8String.TryFormat(buffer.Slice(start), out written), Is.True);
        Assert.That(buffer.Slice(start, written).SequenceEqual("часть".ToCharArray()), Is.True);
    }

    [Test]
    public void ISpanFormattable_Test()
    {
        var utf8str = new Utf8String("моя строка"u8.ToArray());
        
        Assert.That($"Format '{utf8str}' str", Is.EqualTo("Format 'моя строка' str"));

        utf8str = Utf8String.Empty;
        Assert.That($"Format '{utf8str}' str", Is.EqualTo("Format '' str"));
    }

#if NET8_0_OR_GREATER
    [Test]
    public void IUtf8SpanFormattable_Test()
    {
        var utf8str = new Utf8String("моя строка"u8.ToArray());

        Span<byte> bytes = new byte[100];

        Assert.That(System.Text.Unicode.Utf8.TryWrite(bytes, $"Format '{utf8str}' str", out var written), Is.True);

        Assert.That(bytes[..written].SequenceEqual("Format 'моя строка' str"u8), Is.True);
    }
#endif
}