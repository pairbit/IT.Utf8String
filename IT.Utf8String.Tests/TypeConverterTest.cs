using IT;
using System.ComponentModel;

namespace Tests;

public class TypeConverterTest
{
    [Test]
    public void CanConvertFrom_Test()
    {
        var typeConverter = TypeDescriptor.GetConverter(typeof(Utf8String));

        Assert.That(typeConverter.CanConvertFrom(typeof(Utf8String)), Is.True);
        Assert.That(typeConverter.CanConvertFrom(typeof(string)), Is.True);
        Assert.That(typeConverter.CanConvertFrom(typeof(char[])), Is.True);
        Assert.That(typeConverter.CanConvertFrom(typeof(Memory<char>)), Is.True);
        Assert.That(typeConverter.CanConvertFrom(typeof(ReadOnlyMemory<char>)), Is.True);

        Assert.That(typeConverter.CanConvertFrom(typeof(byte[])), Is.True);
        Assert.That(typeConverter.CanConvertFrom(typeof(Memory<byte>)), Is.True);
        Assert.That(typeConverter.CanConvertFrom(typeof(ReadOnlyMemory<byte>)), Is.True);
    }

    [Test]
    public void ConvertFrom_Test()
    {
        var typeConverter = TypeDescriptor.GetConverter(typeof(Utf8String));

        Assert.That(typeConverter.ConvertFromString("str"), Is.EqualTo(Utf8String.Parse("str"u8)));
        Assert.That(typeConverter.ConvertFromInvariantString("InvStr"), Is.EqualTo(Utf8String.Parse("InvStr"u8)));
        Assert.That(typeConverter.ConvertFrom("string"), Is.EqualTo(Utf8String.Parse("string"u8)));

        Assert.That(typeConverter.ConvertFrom("char[]".ToCharArray()), Is.EqualTo(Utf8String.Parse("char[]"u8)));
        Assert.That(typeConverter.ConvertFrom("Memory<char>".ToCharArray().AsMemory()), Is.EqualTo(Utf8String.Parse("Memory<char>"u8)));
        Assert.That(typeConverter.ConvertFrom((ReadOnlyMemory<char>)"ReadOnlyMemory<char>".ToCharArray().AsMemory()), Is.EqualTo(Utf8String.Parse("ReadOnlyMemory<char>"u8)));

        Assert.That(typeConverter.ConvertFrom("byte[]"u8.ToArray()), Is.EqualTo(Utf8String.Parse("byte[]"u8)));
        Assert.That(typeConverter.ConvertFrom("Memory<byte>"u8.ToArray().AsMemory()), Is.EqualTo(Utf8String.Parse("Memory<byte>"u8)));
        Assert.That(typeConverter.ConvertFrom((ReadOnlyMemory<byte>)"ReadOnlyMemory<byte>"u8.ToArray().AsMemory()), Is.EqualTo(Utf8String.Parse("ReadOnlyMemory<byte>"u8)));
    }
}