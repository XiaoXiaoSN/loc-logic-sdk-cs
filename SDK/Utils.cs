using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

public static class Utils
{
    public static string? ConvertValueToJsonString(Value protoValue)
    {
        JsonFormatter formatter = new JsonFormatter(JsonFormatter.Settings.Default.WithIndentation());
        return formatter.Format(protoValue);
    }

    public static JsonNode? ConvertValueToJson(Value protoValue)
    {
        JsonFormatter formatter = new JsonFormatter(JsonFormatter.Settings.Default.WithIndentation());
        string jsonString = formatter.Format(protoValue);
        return JsonSerializer.Deserialize<JsonNode>(jsonString);
    }

    public static Value ConvertObjectToValue(object? obj)
    {
        switch (obj)
        {
            case Value value:
                return value;

            case null:
                return Value.ForNull();

            case int i:
                return Value.ForNumber(i);

            case long l:
                return Value.ForNumber(l);

            case float f:
                return Value.ForNumber(f);

            case double d:
                return Value.ForNumber(d);

            case bool b:
                return Value.ForBool(b);

            case string s:
                return Value.ForString(s);

            case IDictionary<string, object> dict:
                var structValue = new Struct();
                foreach (var kvp in dict)
                {
                    structValue.Fields[kvp.Key] = ConvertObjectToValue(kvp.Value);
                }
                return Value.ForStruct(structValue);

            case IDictionary dict:
                var dictionaryStruct = new Struct();
                foreach (DictionaryEntry kvp in dict)
                {
                    dictionaryStruct.Fields[kvp.Key.ToString()] = ConvertObjectToValue(kvp.Value);
                }
                return Value.ForStruct(dictionaryStruct);

            case IEnumerable<object> list:
                var listValue = list.Select(ConvertObjectToValue);
                return Value.ForList(listValue.ToArray());

            default:
                var customStruct = new Struct();
                System.Type type = obj.GetType();
                foreach (PropertyInfo propertyInfo in type.GetProperties())
                {
                    var propertyValue = propertyInfo.GetValue(obj);
                    customStruct.Fields[propertyInfo.Name] = ConvertObjectToValue(propertyValue);
                }
                return Value.ForStruct(customStruct);
        }
    }

    public static byte[] ConvertToByteArray<T>(T protoStruct)
        where T : IMessage<T>
    {
        using (var ms = new MemoryStream())
        {
            protoStruct.WriteTo(ms);
            return ms.ToArray();
        }
    }

    public static T? ToObject<T>(this byte[] buf)
        where T : IMessage<T>, new()
    {
        if (buf == null)
            return default;

        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(buf, 0, buf.Length);
            ms.Seek(0, SeekOrigin.Begin);

            MessageParser<T> parser = new MessageParser<T>(() => new T());
            return parser.ParseFrom(ms);
        }
    }

    public static UInt128 BytesToUInt128(byte[] bytes)
    {
        if (bytes.Length != 16)
        {
            throw new ArgumentException("Byte array must be exactly 16 bytes long.");
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        ulong upper = BitConverter.ToUInt64(bytes, 8);
        ulong lower = BitConverter.ToUInt64(bytes, 0);

        return new UInt128(upper, lower);
    }

    public static byte[] UInt128ToBytes(UInt128 value)
    {
        byte[] bytes = new byte[16];
        for (int i = 15; i >= 0; i--)
        {
            bytes[i] = (byte)(value % 256);
            value /= 256;
        }
        return bytes;
    }

    public static string EncodeUint128(UInt128 value)
    {
        var bytes = UInt128ToBytes(value);
        string base64 = Convert.ToBase64String(bytes);
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    public static Guid ConvertUuidToGuid(Saffron.Common.Uuid uuid)
    {
        // Variant 2 UUIDs, historically used in Microsoft's COM/OLE libraries,
        // use a little-endian format, but appear mixed-endian with the first three
        // components of the UUID as little-endian and last two big-endian, due to
        // the missing byte dashes when formatted as a string.
        // For example, `00112233-4455-6677-8899-aabbccddeeff` is encoded as the bytes
        // `33 22 11 00 55 44 77 66 88 99 aa bb cc dd ee ff`.
        byte[] bytes = new byte[16];
        for (int i = 7; i >= 0; i--)
        {
            bytes[i] = (byte)(uuid.HighBits % 256);
            uuid.HighBits /= 256;
        }
        (bytes[0], bytes[1], bytes[2], bytes[3]) = (bytes[3], bytes[2], bytes[1], bytes[0]);
        (bytes[4], bytes[5]) = (bytes[5], bytes[4]);
        (bytes[6], bytes[7]) = (bytes[7], bytes[6]);

        for (int i = 7; i >= 0; i--)
        {
            bytes[i + 8] = (byte)(uuid.LowBits % 256);
            uuid.LowBits /= 256;
        }

        return new Guid(bytes);
    }
}
