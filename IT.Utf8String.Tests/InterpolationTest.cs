using IT;

namespace Tests;

public class InterpolationTest
{
    [Test]
    public void SizeOfTest()
    {
        var utf8str = new Utf8String("моя строка"u8.ToArray());
        
        var format = $"Format '{utf8str}' str";

        Assert.That(format, Is.EqualTo("Format 'моя строка' str"));
    }
}