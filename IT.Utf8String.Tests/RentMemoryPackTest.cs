#if NET6_0_OR_GREATER

using MemoryPack;
using MemoryPack.Formatters;
using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Tests;

[MemoryPackable]
public partial class PoolModelSample : IDisposable
{
    internal static bool _toReturnEnabled;

    [ThreadStatic]
    internal static List<PoolModelSample>? _toReturn;//return if exception

    [MemoryPoolFormatter<byte>]
    public Memory<byte> Payload { get; set; }

    public int Id { get; set; }

    // You must write the return code yourself, here is snippet.

    bool usePool;

    [MemoryPackOnDeserialized]
    void OnDeserialized()
    {
        usePool = true;
        if (_toReturnEnabled)
        {
            var toReturn = _toReturn;
            if (toReturn == null)
            {
                toReturn = _toReturn = new List<PoolModelSample>();
            }
            toReturn.Add(this);
        }
    }

    public void Dispose()
    {
        if (!usePool) return;

        Return(Payload); Payload = default;
    }

    static void Return<T>(Memory<T> memory) => Return((ReadOnlyMemory<T>)memory);

    static void Return<T>(ReadOnlyMemory<T> memory)
    {
        if (MemoryMarshal.TryGetArray(memory, out var segment) && segment.Array is { Length: > 0 })
        {
            ArrayPool<T>.Shared.Return(segment.Array, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
    }
}

[MemoryPackable]
public partial class DataByte
{
    public byte[] Payload { get; set; } = null!;

    public byte Id { get; set; }
}

public class PoolModelSampleTest
{
    [Test]
    public void TestPartial()
    {
        var bytes = new byte[1];
        Random.Shared.NextBytes(bytes);
        var binByte = MemoryPackSerializer.Serialize(new DataByte { Payload = bytes, Id = 0 });
        var binInt = MemoryPackSerializer.Serialize(new PoolModelSample { Payload = bytes, Id =0 });
        
        PoolModelSample? model = null;
        try
        {
            var consumed = MemoryPackSerializer.Deserialize(binByte, ref model);
            Assert.Fail();
        }
        catch (MemoryPackSerializationException)
        {

        }
    }

    [Test]
    public void Test()
    {
        var bytes = new byte[1];
        Random.Shared.NextBytes(bytes);
        var models = new[] { new PoolModelSample { Payload = bytes, Id = 0 }, new PoolModelSample { Payload = bytes, Id = 1 } };

        var bin = MemoryPackSerializer.Serialize(models);

        DeserializeTest(models, bin);
        DeserializeTest(models, bin.AsSpan(0, 20));//MemoryPackSerializationException
    }

    private void DeserializeTest(PoolModelSample[] models, ReadOnlySpan<byte> bin)
    {
        PoolModelSample[]? models2 = null;
        try
        {
            MemoryPackSerializer.Deserialize(bin, ref models2);

            Test(models, models2!);
        }
        catch (MemoryPackSerializationException)
        {
            if (models2 != null)
            {
                foreach (var model2 in models2)
                {
                    model2?.Dispose();
                }
            }
        }

        try
        {
            PoolModelSample._toReturnEnabled = true;
            models2 = MemoryPackSerializer.Deserialize<PoolModelSample[]>(bin)!;

            //if success Clear
            var toReturn = PoolModelSample._toReturn;
            if (toReturn != null) toReturn.Clear();

            Test(models, models2);
        }
        catch (MemoryPackSerializationException)
        {
            var toReturn = PoolModelSample._toReturn;
            if (toReturn != null)
            {
                foreach (var item in toReturn)
                {
                    item.Dispose();
                }
                toReturn.Clear();
                return;
            }
            Assert.Fail();
        }
        finally
        {
            PoolModelSample._toReturnEnabled = false;
        }
    }

    private void Test(PoolModelSample[] models, PoolModelSample[] models2)
    {
        foreach (var model2 in models2)
        {
            try
            {
                var model = models[model2.Id];
                Assert.That(model.Id, Is.EqualTo(model2.Id));
                Assert.That(model.Payload.Span.SequenceEqual(model.Payload.Span), Is.True);
            }
            finally
            {
                model2.Dispose();
            }
        }
    }
}

internal static class ArrayPoolShared
{
    private static bool _addToList;

    [ThreadStatic]
    private static List<Action>? _list;

    public static bool IsEnabled => _addToList;

    public static void AddToList()
    {
        _addToList = true;
    }

    public static T?[] Rent<T>(int minimumLength)
    {
        var rented = ArrayPool<T?>.Shared.Rent(minimumLength);
        if (_addToList)
        {
            var list = _list;
            if (list == null)
            {
                list = _list = new List<Action>();
            }

            list.Add(() => ArrayPool<T?>.Shared.Return(rented, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>()));
        }
        return rented;
    }

    public static void ReturnAndClear()
    {
        var list = _list;
        if (list != null && list.Count > 0)
        {
            foreach (var returnToPool in list)
            {
                returnToPool();
            }
            list.Clear();
        }
        _addToList = false;
    }

    public static void Clear()
    {
        var list = _list;
        if (list != null && list.Count > 0)
        {
            list.Clear();
        }
        _addToList = false;
    }
}

#endif