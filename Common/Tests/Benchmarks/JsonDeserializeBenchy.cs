using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.ExtensionMethods;
using O10.Core.Serialization;
using SpanJson;
using SpanJson.Formatters;
using System;
using System.Text;
using System.Diagnostics;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class JsonDeserializeBenchy
    {
        private string _json;
        private string _json2;
        private byte[] _jsonbytes;
        private byte[] _json2bytes;

        private string _jsonSpan;
        private string _jsonSpan2;

        [GlobalSetup]
        public void Init()
        {
            _json = "{\"Issuer\":\"430D8377E96EAA1A93D1DC2C8C64614D9552791BB8D262CAE0DFBA8100DA0DCD\"}";
            _json2 = "{\"Issuer\":\"Qw2Dd+luqhqT0dwsjGRhTZVSeRu40mLK4N+6gQDaDc0=\"}";

            _jsonSpan = "{\"Issuer\":\"430D8377E96EAA1A93D1DC2C8C64614D9552791BB8D262CAE0DFBA8100DA0DCD\"}";
            _jsonSpan2 = "{\"Issuer\":[67,13,131,119,233,110,170,26,147,209,220,44,140,100,97,77,149,82,121,27,184,210,98,202,224,223,186,129,0,218,13,205]}";
            _jsonbytes = Encoding.UTF8.GetBytes(_jsonSpan);
            _json2bytes = Encoding.UTF8.GetBytes(_jsonSpan2);
        }

        [Benchmark]
        public WithByteArrayNoConverter ByteArrayDirectSpanJson()
        {
            return SpanJson.JsonSerializer.Generic.Utf8.Deserialize<WithByteArrayNoConverter>(_json2bytes);
        }

        [Benchmark]
        public WithByteArrayNoConverter ByteArrayDirectNewtonsoft()
        {
            return JsonConvert.DeserializeObject<WithByteArrayNoConverter>(_json2);
        }

        [Benchmark]
        public WithByteArrayNoConverter ByteArrayDirectTextJson()
        {
            return System.Text.Json.JsonSerializer.Deserialize<WithByteArrayNoConverter>(_json2);
        }

        [Benchmark]
        public WithByteArray ByteArraySpanJson()
        {
            return SpanJson.JsonSerializer.Generic.Utf8.Deserialize<WithByteArray>(_jsonbytes);
        }

        [Benchmark]
        public WithByteArray ByteArrayNewtonsoft()
        {
            return JsonConvert.DeserializeObject<WithByteArray>(_json);
        }

        [Benchmark]
        public WithByteArray ByteArrayTextJson()
        {
            return System.Text.Json.JsonSerializer.Deserialize<WithByteArray>(_json);
        }

        [Benchmark]
        public WithMemory MemoryNewtonsoft()
        {
            return JsonConvert.DeserializeObject<WithMemory>(_json);
        }

        [Benchmark]
        public WithMemory MemoryTextJson()
        {
            return System.Text.Json.JsonSerializer.Deserialize<WithMemory>(_json);
        }

        [Benchmark]
        public WithKey KeyNewtonsoft()
        {
            return JsonConvert.DeserializeObject<WithKey>(_json);
        }

        [Benchmark]
        public WithKey KeyTextJson()
        {
            return System.Text.Json.JsonSerializer.Deserialize<WithKey>(_json);
        }

        [Benchmark]
        public WithReadOnlyMemory ReadOnlyMemorySpanJson()
        {
            return SpanJson.JsonSerializer.Generic.Utf8.Deserialize<WithReadOnlyMemory>(_jsonbytes);
        }
    }

    public class WithReadOnlyMemory
    {
        [JsonCustomSerializer(typeof(ReadOnlyMemoryCustomSerializer))]
        public ReadOnlyMemory<byte> Issuer { get; set; }
    }

    public class WithByteArrayNoConverter
    {
        public byte[] Issuer { get; set; }
    }

    public class WithByteArray
    {
        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonByteArrayJsonConverter))]
        [JsonCustomSerializer(typeof(ByteArrayCustomSerializer))]
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] Issuer { get; set; }
    }

    public class WithMemory
    {
        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonMemoryJsonConverter))]
        [JsonConverter(typeof(MemoryByteJsonConverter))]
        public Memory<byte> Issuer { get; set; }
    }

    public class WithKey
    {
        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonKeyJsonConverter))]
        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey Issuer { get; set; }
    }

    public class ByteArrayCustomSerializer : ICustomJsonFormatter<byte[]>
    {
        public static readonly ByteArrayCustomSerializer Default = new();

        public object Arguments { get; set; }

        public byte[] Deserialize(ref JsonReader<byte> reader)
        {
            var value = StringUtf8Formatter.Default.Deserialize(ref reader);

            return value.HexStringToByteArray();
        }

        public byte[] Deserialize(ref JsonReader<char> reader)
        {
            var value = StringUtf16Formatter.Default.Deserialize(ref reader);
            return value.HexStringToByteArray();
        }

        public void Serialize(ref JsonWriter<byte> writer, byte[] value)
        {
            StringUtf8Formatter.Default.Serialize(ref writer, value.ToHexString());
        }

        public void Serialize(ref JsonWriter<char> writer, byte[] value)
        {
            StringUtf16Formatter.Default.Serialize(ref writer, value.ToHexString());
        }
    }

    public class ReadOnlyMemoryCustomSerializer : ICustomJsonFormatter<ReadOnlyMemory<byte>>
    {
        public static readonly ReadOnlyMemoryCustomSerializer Default = new();

        public object Arguments { get; set; }

        public ReadOnlyMemory<byte> Deserialize(ref JsonReader<byte> reader)
        {
            Debugger.Break();
            var value = StringUtf8Formatter.Default.Deserialize(ref reader);

            return new ReadOnlyMemory<byte>(value.HexStringToByteArray());
        }

        public ReadOnlyMemory<byte> Deserialize(ref JsonReader<char> reader)
        {
            Debugger.Break();
            var value = StringUtf16Formatter.Default.Deserialize(ref reader);
            return new ReadOnlyMemory<byte>(value.HexStringToByteArray());
        }

        public void Serialize(ref JsonWriter<byte> writer, ReadOnlyMemory<byte> value)
        {
            Debugger.Break();
            StringUtf8Formatter.Default.Serialize(ref writer, value.ToHexString());
        }

        public void Serialize(ref JsonWriter<char> writer, ReadOnlyMemory<byte> value)
        {
            Debugger.Break();
            StringUtf16Formatter.Default.Serialize(ref writer, value.ToHexString());
        }
    }

    public class TextJsonByteArrayJsonConverter : System.Text.Json.Serialization.JsonConverter<byte[]>
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.Name == "Byte[]";
        }

        public override byte[] Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            string value = reader.GetString();

            if (!string.IsNullOrEmpty(value))
            {
                return value.HexStringToByteArray();
            }

            return null;
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, byte[] value, System.Text.Json.JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToHexString());
        }
    }

    public class TextJsonMemoryJsonConverter : System.Text.Json.Serialization.JsonConverter<Memory<byte>>
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Memory<byte>).IsAssignableFrom(objectType);
        }

        public override Memory<byte> Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            string value = reader.GetString();

            if (!string.IsNullOrEmpty(value))
            {
                return value.HexStringToByteArray();
            }

            return null;
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, Memory<byte> value, System.Text.Json.JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToHexString());
        }
    }

    public class TextJsonKeyJsonConverter : System.Text.Json.Serialization.JsonConverter<IKey>
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IKey).IsAssignableFrom(objectType);
        }

        public override IKey Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            string value = reader.GetString();

            if (!string.IsNullOrEmpty(value))
            {
                return new Key32(value.HexStringToByteArray());
            }

            return null;
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, IKey value, System.Text.Json.JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString());
        }
    }
}
