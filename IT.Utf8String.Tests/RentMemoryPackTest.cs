#if NET6_0_OR_GREATER

using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Tests;

[MemoryPackable]
public partial class PoolModelSample : IDisposable
{
    [ThreadStatic]
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
public partial class PoolModel : IDisposable
{
    [MemoryPoolFormatter2<byte>]
    public Memory<byte> PayloadBytes { get; set; }

    [MemoryPoolFormatter2<int>]
    public Memory<int> PayloadInts { get; set; }

    public int Id { get; set; }

    // You must write the return code yourself, here is snippet.

    bool usePool;

    [MemoryPackOnDeserialized]
    void OnDeserialized()
    {
        usePool = true;
    }

    public void Dispose()
    {
        if (!usePool) return;

        Return(PayloadBytes); PayloadBytes = default;
        Return(PayloadInts); PayloadInts = default;
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
public partial class ModelByte
{
    public byte[] PayloadBytes { get; set; } = null!;

    public int[] PayloadInts { get; set; } = null!;

    public byte Id { get; set; }
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
    public void TestPartial_Work()
    {
        var bytes = new byte[1];
        Random.Shared.NextBytes(bytes);
        var ints = new int[1];
        ints[0] = 3;
        var binByte = MemoryPackSerializer.Serialize(new ModelByte { PayloadBytes = bytes, PayloadInts = ints, Id = 0 });
        var binInt = MemoryPackSerializer.Serialize(new PoolModel { PayloadBytes = bytes, PayloadInts = ints, Id = 0 });

        PoolModel? model = null;
        ArrayPoolShared.AddToList();
        try
        {
            var consumed = MemoryPackSerializer.Deserialize(binByte, ref model);
            ArrayPoolShared.Clear();
        }
        catch (MemoryPackSerializationException)
        {
            ArrayPoolShared.ReturnAndClear();
        }
    }

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
    [ThreadStatic]
    private static State? _threadStaticState;

    public static bool IsEnabled => _threadStaticState != null && _threadStaticState._addToList;

    public static void AddToList()
    {
        var state = _threadStaticState;
        if (state == null)
        {
            state = _threadStaticState = new State();
        }
        state._addToList = true;
    }

    public static T[] Rent<T>(int minimumLength)
    {
        var rented = ArrayPool<T>.Shared.Rent(minimumLength);
        var state = _threadStaticState;
        if (state != null && state._addToList)
        {
            state._list.Add((rented, Return<T>));
        }
        return rented;
    }

    private static void Return<T>(Array array)
    {
        ArrayPool<T>.Shared.Return((T[])array, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    }

    public static void ReturnAndClear()
    {
        var state = _threadStaticState;
        if (state != null)
        {
            var list = state._list;
            if (list.Count > 0)
            {
                foreach (var (array, returnToPool) in list)
                {
                    returnToPool(array);
                }
                list.Clear();
            }
            state._addToList = false;
        }
    }

    public static void Clear()
    {
        var state = _threadStaticState;
        if (state != null)
        {
            var list = state._list;
            if (list.Count > 0)
            {
                list.Clear();
            }
            state._addToList = false;
        }
    }

    class State
    {
        public bool _addToList;
        public readonly List<(Array, Action<Array>)> _list = [];
    }
}

public sealed class MemoryPoolFormatter2Attribute<T> : MemoryPackCustomFormatterAttribute<MemoryPoolFormatter2<T>, Memory<T?>>
{
    private static readonly MemoryPoolFormatter2<T> _formatter = new();

    public override MemoryPoolFormatter2<T> GetFormatter() => _formatter;
}

[Preserve]
public sealed class MemoryPoolFormatter2<T> : MemoryPackFormatter<Memory<T?>>
{
    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Memory<T?> value)
    {
        writer.WriteSpan(value.Span);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref Memory<T?> value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = Memory<T?>.Empty;
            return;
        }

        var memory = ArrayPoolShared.Rent<T?>(length).AsMemory(0, length);
        var span = memory.Span;
        reader.ReadSpanWithoutReadLengthHeader(length, ref span);
        value = memory;
    }
}

#endif