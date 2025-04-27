using IT;

namespace Tests;

public class FormattableTest
{
    [Test]
    public void TryFormat_Test()
    {
        var utf8Memory = new Utf8Memory("моя строка"u8.ToArray());
        Span<char> buffer = stackalloc char[100];

        Assert.That(utf8Memory.TryFormat(buffer, out var written), Is.True);
        Assert.That(buffer.Slice(0, written).SequenceEqual("моя строка".ToCharArray()), Is.True);

        var start = written;
        var str = "моя часть строки"u8.ToArray();
        var sliced = str.AsMemory().Slice(7, 10);
        utf8Memory = new Utf8Memory(sliced);
        Assert.That(utf8Memory.ToString(), Is.EqualTo("часть"));
        Assert.That(utf8Memory.ToChars(), Is.EqualTo("часть".ToCharArray()));

        Assert.That(utf8Memory.TryFormat(buffer.Slice(start), out written), Is.True);
        Assert.That(buffer.Slice(start, written).SequenceEqual("часть".ToCharArray()), Is.True);

        Assert.That(Utf8Memory.Empty.TryFormat(buffer, out written), Is.True);
        Assert.That(written, Is.EqualTo(0));
    }

    [Test]
    public void RO_TryFormat_Test()
    {
        var utf8Memory = new ReadOnlyUtf8Memory("моя строка"u8.ToArray());
        Span<char> buffer = stackalloc char[100];

        Assert.That(utf8Memory.TryFormat(buffer, out var written), Is.True);
        Assert.That(buffer.Slice(0, written).SequenceEqual("моя строка".ToCharArray()), Is.True);

        var start = written;
        var str = "моя часть строки"u8.ToArray();
        var sliced = str.AsMemory().Slice(7, 10);
        utf8Memory = new ReadOnlyUtf8Memory(sliced);
        Assert.That(utf8Memory.ToString(), Is.EqualTo("часть"));
        Assert.That(utf8Memory.ToChars(), Is.EqualTo("часть".ToCharArray()));

        Assert.That(utf8Memory.TryFormat(buffer.Slice(start), out written), Is.True);
        Assert.That(buffer.Slice(start, written).SequenceEqual("часть".ToCharArray()), Is.True);

        Assert.That(ReadOnlyUtf8Memory.Empty.TryFormat(buffer, out written), Is.True);
        Assert.That(written, Is.EqualTo(0));
    }

    [Test]
    public void ISpanFormattable_Test()
    {
        var utf8Memory = new Utf8Memory("моя строка"u8.ToArray());
        
        Assert.That($"Format '{utf8Memory}' str", Is.EqualTo("Format 'моя строка' str"));

        utf8Memory = Utf8Memory.Empty;
        Assert.That($"Format '{utf8Memory}' str", Is.EqualTo("Format '' str"));
    }

    [Test]
    public void RO_ISpanFormattable_Test()
    {
        var utf8Memory = new ReadOnlyUtf8Memory("моя строка"u8.ToArray());

        Assert.That($"Format '{utf8Memory}' str", Is.EqualTo("Format 'моя строка' str"));

        utf8Memory = ReadOnlyUtf8Memory.Empty;
        Assert.That($"Format '{utf8Memory}' str", Is.EqualTo("Format '' str"));
    }

#if NET8_0_OR_GREATER
    [Test]
    public void IUtf8SpanFormattable_Test()
    {
        var utf8Memory = new Utf8Memory("моя строка"u8.ToArray());

        Span<byte> bytes = new byte[100];

        Assert.That(System.Text.Unicode.Utf8.TryWrite(bytes, $"Format '{utf8Memory}' str", out var written), Is.True);

        Assert.That(bytes[..written].SequenceEqual("Format 'моя строка' str"u8), Is.True);
    }

    [Test]
    public void RO_IUtf8SpanFormattable_Test()
    {
        var utf8Memory = new ReadOnlyUtf8Memory("моя строка"u8.ToArray());

        Span<byte> bytes = new byte[100];

        Assert.That(System.Text.Unicode.Utf8.TryWrite(bytes, $"Format '{utf8Memory}' str", out var written), Is.True);

        Assert.That(bytes[..written].SequenceEqual("Format 'моя строка' str"u8), Is.True);
    }
#endif
}