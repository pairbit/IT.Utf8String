using IT;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Tests;

public class JsonTest
{
    record Entity
    {
        public string? Str { get; set; }

        public Utf8Memory? Utf8Str { get; set; }
    }

    [Test]
    public void BaseTest()
    {
        Assert.That(JsonSerializer.Serialize("my utf8 str"),
            Is.EqualTo("\"my utf8 str\""));

        Assert.That(JsonSerializer.Serialize(new Utf8Memory("my utf8 str"u8.ToArray())),
            Is.EqualTo("\"my utf8 str\""));

        Assert.That(JsonSerializer.Serialize(new ReadOnlyUtf8Memory("my utf8 str"u8.ToArray())),
            Is.EqualTo("\"my utf8 str\""));

        Assert.That(JsonSerializer.Deserialize<string>("\"my utf8 str\""),
            Is.EqualTo("my utf8 str"));

        Assert.That(JsonSerializer.Deserialize<Utf8Memory>("\"my utf8 str\""),
            Is.EqualTo(new Utf8Memory("my utf8 str"u8.ToArray())));

        Assert.That(JsonSerializer.Deserialize<ReadOnlyUtf8Memory>("\"my utf8 str\""),
            Is.EqualTo(new ReadOnlyUtf8Memory("my utf8 str"u8.ToArray())));
    }

    [Test]
    public void NullTest()
    {
        Assert.That(JsonSerializer.Serialize<string?>(null),
            Is.EqualTo("null"));

        Assert.That(JsonSerializer.Deserialize<string>("null"),
            Is.EqualTo(null));

        Assert.That(JsonSerializer.Serialize(string.Empty),
            Is.EqualTo("\"\""));

        Assert.That(JsonSerializer.Deserialize<string>("\"\""),
            Is.EqualTo(string.Empty));

        Assert.That(JsonSerializer.Serialize(Utf8Memory.Empty),
            Is.EqualTo("\"\""));

        Assert.That(JsonSerializer.Deserialize<Utf8Memory>("\"\""),
            Is.EqualTo(Utf8Memory.Empty));

        Assert.That(JsonSerializer.Deserialize<Utf8Memory>("null"),
            Is.EqualTo(Utf8Memory.Empty));

        Assert.That(JsonSerializer.Serialize(ReadOnlyUtf8Memory.Empty),
            Is.EqualTo("\"\""));

        Assert.That(JsonSerializer.Deserialize<ReadOnlyUtf8Memory>("\"\""),
            Is.EqualTo(ReadOnlyUtf8Memory.Empty));

        Assert.That(JsonSerializer.Deserialize<ReadOnlyUtf8Memory>("null"),
            Is.EqualTo(ReadOnlyUtf8Memory.Empty));
    }

    [Test]
    public void BaseEscapedTest()
    {
        Assert.That(JsonSerializer.Serialize("my \"utf8\" str"),
            Is.EqualTo("\"my \\u0022utf8\\u0022 str\""));

        Assert.That(JsonSerializer.Serialize(new Utf8Memory("my \"utf8\" str"u8.ToArray())),
            Is.EqualTo("\"my \\u0022utf8\\u0022 str\""));

        Assert.That(JsonSerializer.Serialize(new ReadOnlyUtf8Memory("my \"utf8\" str"u8.ToArray())),
            Is.EqualTo("\"my \\u0022utf8\\u0022 str\""));

        Assert.That(JsonSerializer.Deserialize<string>("\"my \\u0022utf8\\u0022 str\""),
            Is.EqualTo("my \"utf8\" str"));

        Assert.That(JsonSerializer.Deserialize<Utf8Memory>("\"my \\u0022utf8\\u0022 str\""),
            Is.EqualTo(new Utf8Memory("my \"utf8\" str"u8.ToArray())));

        Assert.That(JsonSerializer.Deserialize<ReadOnlyUtf8Memory>("\"my \\u0022utf8\\u0022 str\""),
            Is.EqualTo(new ReadOnlyUtf8Memory("my \"utf8\" str"u8.ToArray())));

        Assert.That(JsonSerializer.Deserialize<string>("\"my \\\"utf8\\\" str\""),
            Is.EqualTo("my \"utf8\" str"));

        Assert.That(JsonSerializer.Deserialize<Utf8Memory>("\"my \\\"utf8\\\" str\""),
            Is.EqualTo(new Utf8Memory("my \"utf8\" str"u8.ToArray())));

        Assert.That(JsonSerializer.Deserialize<ReadOnlyUtf8Memory>("\"my \\\"utf8\\\" str\""),
            Is.EqualTo(new ReadOnlyUtf8Memory("my \"utf8\" str"u8.ToArray())));

        var jso = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        Assert.That(JsonSerializer.Serialize("my \"utf8\" str", jso), 
            Is.EqualTo("\"my \\\"utf8\\\" str\""));

        Assert.That(JsonSerializer.Serialize(new Utf8Memory("my \"utf8\" str"u8.ToArray()), jso), 
            Is.EqualTo("\"my \\\"utf8\\\" str\""));

        Assert.That(JsonSerializer.Serialize(new ReadOnlyUtf8Memory("my \"utf8\" str"u8.ToArray()), jso),
            Is.EqualTo("\"my \\\"utf8\\\" str\""));
    }

    [Test]
    public void SerializeTest()
    {
        var obj = new Entity() { Str = "my utf8 str", Utf8Str = "my utf8 str"u8.ToArray() };
        var json = JsonSerializer.Serialize(obj);

        Assert.That(json, Is.EqualTo("{\"Str\":\"my utf8 str\",\"Utf8Str\":\"my utf8 str\"}"));
        Assert.That(JsonSerializer.Deserialize<Entity>(json), Is.EqualTo(obj));

        obj = new Entity();
        json = JsonSerializer.Serialize(obj);

        Assert.That(json, Is.EqualTo("{\"Str\":null,\"Utf8Str\":null}"));
        Assert.That(JsonSerializer.Deserialize<Entity>(json), Is.EqualTo(obj));

        obj = new Entity() { Str = "моя utf8 строка", Utf8Str = "моя utf8 строка"u8.ToArray() };
        json = JsonSerializer.Serialize(obj);

        Assert.That(json, Is.EqualTo("{\"Str\":\"\\u043C\\u043E\\u044F utf8 \\u0441\\u0442\\u0440\\u043E\\u043A\\u0430\",\"Utf8Str\":\"\\u043C\\u043E\\u044F utf8 \\u0441\\u0442\\u0440\\u043E\\u043A\\u0430\"}"));
        Assert.That(JsonSerializer.Deserialize<Entity>(json), Is.EqualTo(obj));

        json = "{\"Str\":\"\\u043C\\u043E\\u044F utf8 \\u0441\\u0442\\u0440\\u043E\\u043A\\u0430\",\"Utf8Str\":\"моя utf8 строка\"}";

        Assert.That(JsonSerializer.Deserialize<Entity>(json), Is.EqualTo(obj));
    }

    record ROEntity
    {
        public string? Str { get; set; }

        public ReadOnlyUtf8Memory? Utf8Str { get; set; }
    }

    [Test]
    public void RO_SerializeTest()
    {
        var obj = new ROEntity() { Str = "my utf8 str", Utf8Str = "my utf8 str"u8.ToArray() };
        var json = JsonSerializer.Serialize(obj);

        Assert.That(json, Is.EqualTo("{\"Str\":\"my utf8 str\",\"Utf8Str\":\"my utf8 str\"}"));
        Assert.That(JsonSerializer.Deserialize<ROEntity>(json), Is.EqualTo(obj));

        obj = new ROEntity();
        json = JsonSerializer.Serialize(obj);

        Assert.That(json, Is.EqualTo("{\"Str\":null,\"Utf8Str\":null}"));
        Assert.That(JsonSerializer.Deserialize<ROEntity>(json), Is.EqualTo(obj));

        obj = new ROEntity() { Str = "моя utf8 строка", Utf8Str = "моя utf8 строка"u8.ToArray() };
        json = JsonSerializer.Serialize(obj);

        Assert.That(json, Is.EqualTo("{\"Str\":\"\\u043C\\u043E\\u044F utf8 \\u0441\\u0442\\u0440\\u043E\\u043A\\u0430\",\"Utf8Str\":\"\\u043C\\u043E\\u044F utf8 \\u0441\\u0442\\u0440\\u043E\\u043A\\u0430\"}"));
        Assert.That(JsonSerializer.Deserialize<ROEntity>(json), Is.EqualTo(obj));

        json = "{\"Str\":\"\\u043C\\u043E\\u044F utf8 \\u0441\\u0442\\u0440\\u043E\\u043A\\u0430\",\"Utf8Str\":\"моя utf8 строка\"}";

        Assert.That(JsonSerializer.Deserialize<ROEntity>(json), Is.EqualTo(obj));
    }

    [Test]
    public void SizeOfTest()
    {
        Assert.That(Unsafe.SizeOf<Utf8Memory>(), Is.EqualTo(16));
        Assert.That(Unsafe.SizeOf<ReadOnlyUtf8Memory>(), Is.EqualTo(16));
    }
}