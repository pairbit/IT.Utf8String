using IT;

namespace Tests;

internal class ComparableTest
{
    [Test]
    public void Test()
    {
        var strings = "str1,str1";
        var index = strings.IndexOf(',');
        var str1 = strings.Substring(0, index);
        var str2 = strings.Substring(index + 1);

        var set = new HashSet<string>();

        Assert.That(ReferenceEquals(str1, str2), Is.False);
        Assert.That(str1 == str2, Is.True);
        Assert.That(str1.GetHashCode(), Is.EqualTo(str2.GetHashCode()));

        Assert.That(set.Add(str1), Is.True);
        Assert.That(set.Add(str2), Is.False);
    }

    [Test]
    public void SortTest()
    {
        var array1 = new Utf8String[] {
            "str3"u8.ToArray(),
            "str1"u8.ToArray(),
            "str2"u8.ToArray()
        };
        var array2 = new ReadOnlyUtf8String[] {
            "str2"u8.ToArray(),
            "str3"u8.ToArray(),
            "str1"u8.ToArray()
        };

        Array.Sort(array1);
        Array.Sort(array2);

        Assert.That(array1.Length, Is.EqualTo(array2.Length));

        for (int i = 0; i < array1.Length; i++)
        {
            Assert.That(array1[i].AsReadOnly(), Is.EqualTo(array2[i]));
        }
    }

    [Test]
    public void Utf8Test()
    {
        var strings = (Utf8String)"ssdfsdfsd3532423rsdefsdfsdfsr235sfsdfsdfsdftr1,ssdfsdfsd3532423rsdefsdfsdfsr235sfsdfsdfsdftr1"u8.ToArray();
        var index = strings.Span.IndexOf((byte)',');
        var str1 = strings.Slice(0, index);
        var str2 = strings.Slice(index + 1);

        //Assert.That(ReferenceEquals(str1, str2), Is.False);

        HashSetTest(str1, str2);
        RO_HashSetTest(str1, str2);
    }

    public void RO_HashSetTest(ReadOnlyUtf8String str1, Utf8String str2)
    {
        var set = new HashSet<ReadOnlyUtf8String>();

        Assert.That(str1 == str2, Is.True);
        Assert.That(str1.GetHashCode(), Is.EqualTo(str2.GetHashCode()));

        Assert.That(set.Add(str1), Is.True);
        Assert.That(set.Add(str2), Is.False);
    }

    public void HashSetTest(Utf8String str1, Utf8String str2)
    {
        var set = new HashSet<Utf8String>();

        Assert.That(str1 == str2, Is.True);
        Assert.That(str1.GetHashCode(), Is.EqualTo(str2.GetHashCode()));

        Assert.That(set.Add(str1), Is.True);
        Assert.That(set.Add(str2), Is.False);
    }
}