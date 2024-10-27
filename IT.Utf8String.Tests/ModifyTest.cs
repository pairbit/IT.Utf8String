using IT;
using System.Buffers;
using System.Buffers.Text;

namespace Tests;

public class ModifyTest
{
    [Test]
    public void EmptyTest()
    {
        Assert.That(Utf8String.Empty.ToString(), Is.EqualTo(""));
        Assert.That(Utf8String.Empty.ToString(), Is.EqualTo(string.Empty));
        Assert.That(ReferenceEquals(Utf8String.Empty.ToString(), string.Empty), Is.True);
    }

    [Test]
    public void Change_Test()
    {
        var utf8String = new Utf8String("моя 0 строка"u8.ToArray());

        Assert.That(utf8String.ToString(), Is.EqualTo("моя 0 строка"));

        utf8String.Span[7] = (byte)'1';
        Assert.That(utf8String.ToString(), Is.EqualTo("моя 1 строка"));
    }

    [Test]
    public void Base64_Test()
    {
        var data = "моя строка в base64"u8;
        var base64 = new byte[Base64.GetMaxEncodedToUtf8Length(data.Length)];
        Assert.That(Base64.EncodeToUtf8(data, base64, out _, out _), Is.EqualTo(OperationStatus.Done));

        var utf8String = new Utf8String(base64);
        Assert.That(utf8String.ToString(), Is.EqualTo("0LzQvtGPINGB0YLRgNC+0LrQsCDQsiBiYXNlNjQ="));

        Assert.That(Base64.DecodeFromUtf8InPlace(utf8String.Span, out var written), Is.EqualTo(OperationStatus.Done));
        Assert.That(utf8String.Slice(0, written).ToString(), Is.EqualTo("моя строка в base64"));

        Assert.That(utf8String.ToString(), Is.EqualTo("моя строка в base64iBiYXNlNjQ="));
    }
}