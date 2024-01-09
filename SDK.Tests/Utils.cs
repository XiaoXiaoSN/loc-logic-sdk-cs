namespace SDK.Tests;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.Text.Json.Nodes;

public class TestUtils
{
    [Fact]
    public void Test_ConvertValueToJsonString()
    {
        // test string
        var protoString = Value.ForString("Hello, World!");
        var str = Utils.ConvertValueToJsonString(protoString);
        Assert.Equal("\"Hello, World!\"", str);

        // test number
        var protoNumber = Value.ForNumber(123.45);
        var str2 = Utils.ConvertValueToJsonString(protoNumber);
        Assert.Equal("123.45", str2);

        // test bool
        var protoBoolean = Value.ForBool(true);
        var str3 = Utils.ConvertValueToJsonString(protoBoolean);
        Assert.Equal("true", str3);

        // test null
        var protoNull = Value.ForNull();
        var str4 = Utils.ConvertValueToJsonString(protoNull);
        Assert.Equal("null", str4);

        // test struct
        var protoStruct = Value.ForStruct(new Struct
        {
            Fields =
            {
                { "key1", Value.ForString("value1") },
                { "key2", Value.ForNumber(123) },
                { "key3", Value.ForString("2019-08-01T00:00:00") },
            },
        });
        var str5 = Utils.ConvertValueToJsonString(protoStruct);
        Assert.Equal("{\n  \"key1\": \"value1\",\n  \"key2\": 123,\n  \"key3\": \"2019-08-01T00:00:00\"\n}", str5);
    }

    [Fact]
    public void Test_ConvertValueToObject()
    {
        // test string
        var protoString = Value.ForString("Hello, World!");
        var obj = Utils.ConvertValueToJson(protoString);
        Assert.Equal("Hello, World!", (string)obj!);

        // test number
        var protoNumber = Value.ForNumber(123.45);
        var obj2 = Utils.ConvertValueToJson(protoNumber);
        Assert.Equal(123.45, (double)obj2!);

        // test bool
        var protoBoolean = Value.ForBool(true);
        var obj3 = Utils.ConvertValueToJson(protoBoolean);
        Assert.True((bool)obj3!);

        // test null
        var protoNull = Value.ForNull();
        var obj4 = Utils.ConvertValueToJson(protoNull);
        Assert.Null(obj4);

        // test struct
        var protoStruct = Value.ForStruct(new Struct
        {
            Fields =
            {
                { "key1", Value.ForString("value1") },
                { "key2", Value.ForNumber(123) },
                { "key3", Value.ForString("2019-08-01T00:00:00") },
            },
        });
        var obj5 = Utils.ConvertValueToJson(protoStruct);
        Assert.NotNull(obj5);
        Assert.Equal("value1", (string)obj5!["key1"]!);
        Assert.Equal(123, (int)obj5!["key2"]!);
    }

    [Fact]
    public void Test_ConvertObjectToValue()
    {
        // test null
        var result = Utils.ConvertObjectToValue(null);
        Assert.Equal(Value.KindOneofCase.NullValue, result.KindCase);

        // test number
        var result2 = Utils.ConvertObjectToValue(100);
        Assert.Equal(100, result2.NumberValue);

        // test object
        var result3 = Utils.ConvertObjectToValue(new ExampleObject { Name = "Meow", Age = 100 });
        var expected = new Struct
        {
            Fields =
            {
                { "Name", Value.ForString("Meow") },
                { "Age", Value.ForNumber(100) },
            },
        };
        Assert.Equal(expected, result3.StructValue);

        // test list
        var list = new List<object> { "item1", "item2" };
        var result4 = Utils.ConvertObjectToValue(list);
        Assert.Equal("item1", result4.ListValue.Values[0].StringValue);
        Assert.Equal("item2", result4.ListValue.Values[1].StringValue);

        // test Dictionary
        var dict = new Dictionary<string, object> {
            { "key1", "value1" },
            { "key2", 123 },
        };
        var result5 = Utils.ConvertObjectToValue(dict);
        Assert.Equal("value1", result5.StructValue.Fields["key1"].StringValue);
        Assert.Equal(123, result5.StructValue.Fields["key2"].NumberValue);

        // test Value
        var protoStruct = Value.ForStruct(new Struct
        {
            Fields =
                    {
                        { "key1", Value.ForString("value1") },
                        { "key2", Value.ForNumber(123) },
                        { "key3", Value.ForString("2019-08-01T00:00:00") },
                    },
        });
        var result6 = Utils.ConvertObjectToValue(protoStruct);
        Assert.Equal("value1", result6.StructValue.Fields["key1"].StringValue);
        Assert.Equal(123, result6.StructValue.Fields["key2"].NumberValue);
    }

    [Fact]
    public void Test_BytesToUInt128()
    {
        var expected1 = UInt128.Zero;
        byte[] bytes1 = new byte[16];
        Assert.Equal(expected1, Utils.BytesToUInt128(bytes1));

        var expected2 = UInt128.Parse("168936194220010199234602573438346025250");
        byte[] bytes2 = new byte[] {
            0x7F, 0x17, 0xED, 0x71, 0x8D, 0x45, 0x4A, 0x39, 0x85, 0xFD, 0x9A, 0x94, 0xB9, 0x7B,
            0x59, 0x22
        };
        Assert.Equal(expected2, Utils.BytesToUInt128(bytes2));

        // invalid length
        byte[] bytes3 = new byte[15];
        Assert.Throws<ArgumentException>(() => Utils.BytesToUInt128(bytes3));
    }

    [Fact]
    public void Test_UInt128ToBytes()
    {
        var expected1 = new byte[16];
        var value1 = UInt128.Zero;
        Assert.Equal(expected1, Utils.UInt128ToBytes(value1));

        var expected2 = new byte[] {
            0x7F, 0x17, 0xED, 0x71, 0x8D, 0x45, 0x4A, 0x39, 0x85, 0xFD, 0x9A, 0x94, 0xB9, 0x7B,
            0x59, 0x22
        };
        var value2 = UInt128.Parse("168936194220010199234602573438346025250");
        Assert.Equal(expected2, Utils.UInt128ToBytes(value2));
    }

    [Fact]
    public void Test_EncodeUint128()
    {
        var expected1 = "AAAAAAAAAAAAAAAAAAAAAA";
        var value1 = UInt128.Zero;
        Assert.Equal(expected1, Utils.EncodeUint128(value1));

        var expected2 = "ZX_AgxP-l0Vc2ta04_M8Tg";
        var value2 = UInt128.Parse("134915353883031613463100331535981821006");
        Assert.Equal(expected2, Utils.EncodeUint128(value2));

        var expected3 = "fxftcY1FSjmF_ZqUuXtZIg";
        var value3 = UInt128.Parse("168936194220010199234602573438346025250");
        Assert.Equal(expected3, Utils.EncodeUint128(value3));
    }

    [Fact]
    public void Test_ConvertUuidToGuid()
    {
        var expected1 = new Guid("78b9906e-8bf6-41cc-930c-539a4abfcec0");
        var value1 = new Saffron.Common.Uuid
        {
            HighBits = 8699142959711797708,
            LowBits = 10595935945440087744,
        };
        Assert.Equal(expected1, Utils.ConvertUuidToGuid(value1));
    }
}

class ExampleObject
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
}
