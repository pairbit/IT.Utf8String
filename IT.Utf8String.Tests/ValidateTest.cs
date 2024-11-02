#if NET8_0_OR_GREATER

using System.Text;
using System.Text.Unicode;

namespace Tests;

public class ValidateTest
{
    [Test]
    public void Utf8Valid_Test()
    {
        var ru = GetEncoding(1251);
        var utf8 = Encoding.UTF8;

        var mystr = "моя строка";
        var rubytes = ru.GetBytes(mystr);
        Assert.That(rubytes.Length, Is.EqualTo(10));
        Assert.That(ru.GetString(rubytes), Is.EqualTo(mystr));
        Assert.That(Utf8.IsValid(rubytes), Is.False);

        var utf8Bytes = utf8.GetBytes(mystr);
        Assert.That(utf8Bytes.Length, Is.EqualTo(19));
        Assert.That(utf8.GetString(utf8Bytes), Is.EqualTo(mystr));
        Assert.That(Utf8.IsValid(utf8Bytes), Is.True);

        Assert.That(Utf8.IsValid("utf8"u8), Is.True);
        Assert.That(Utf8.IsValid("утф"u8), Is.True);
        Assert.That(Utf8.IsValid("�"u8), Is.True);
        Assert.That(Utf8.IsValid("􀀀"u8), Is.True);
        Assert.That(Utf8.IsValid("􏿿"u8), Is.True);
    }

    private static Encoding GetEncoding(int codepage)
    {
        try
        {
            return Encoding.GetEncoding(codepage);
        }
        catch (NotSupportedException)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding(codepage);
        }
    }
}

#endif