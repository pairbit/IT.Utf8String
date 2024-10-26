using System.Text.Unicode;

namespace Tests;

public class ValidateTest
{
    [Test]
    public void Utf8Valid_Test()
    {
        Assert.That(Utf8.IsValid("utf8"u8), Is.True);
        Assert.That(Utf8.IsValid("утф"u8), Is.True);
        Assert.That(Utf8.IsValid("�"u8), Is.True);
        Assert.That(Utf8.IsValid("􀀀"u8), Is.True);
        Assert.That(Utf8.IsValid("􏿿"u8), Is.True);
    }
}