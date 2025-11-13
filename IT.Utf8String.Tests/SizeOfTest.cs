using IT;
using System.Runtime.CompilerServices;

namespace Tests;

public class SizeOfTest
{
    [Test]
    public void Test()
    {
        Assert.That(Unsafe.SizeOf<Utf8String>(), Is.EqualTo(8));
        Assert.That(Unsafe.SizeOf<Utf8Memory>(), Is.EqualTo(16));
        Assert.That(Unsafe.SizeOf<ReadOnlyUtf8Memory>(), Is.EqualTo(16));
    }
}