using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.ExtensionMethods;
using O10.Core.Serialization;
using SpanJson;
using SpanJson.Formatters;
using System;
using System.Text;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class JsonDeserializeBenchy
    {
        private string _json;
        private string _json2;
        private byte[] _jsonbytes;
        private byte[] _json2bytes;

        [GlobalSetup]
        public void Init()
        {
            _json = "{\"issuer\":\"430D8377E96EAA1A93D1DC2C8C64614D9552791BB8D262CAE0DFBA8100DA0DCD\"}";
            _json2 = "{\"issuer\":\"Qw2Dd+luqhqT0dwsjGRhTZVSeRu40mLK4N+6gQDaDc0=\"}";
            _jsonbytes = Encoding.UTF8.GetBytes(_json);
            _json2bytes = Encoding.UTF8.GetBytes(_json2);
        }

        [Benchmark]
        public WithByteArrayNoConverter ByteArrayDirectSpanJson()
        {
            return SpanJson.JsonSerializer.Generic.Utf8.Deserialize<WithByteArrayNoConverter>(_json2bytes);
        }

        [Benchmark]
        public WithByteArrayNoConverter ByteArrayDirect()
        {
            return JsonConvert.DeserializeObject<WithByteArrayNoConverter>(_json2);
        }

        [Benchmark]
        public WithByteArray ByteArraySpanJson()
        {
            return SpanJson.JsonSerializer.Generic.Utf8.Deserialize<WithByteArray>(_jsonbytes);
        }

        [Benchmark]
        public WithByteArray ByteArray()
        {
            return JsonConvert.DeserializeObject<WithByteArray>(_json);
        }

        [Benchmark]
        public WithMemory Memory()
        {
            return JsonConvert.DeserializeObject<WithMemory>(_json);
        }

        [Benchmark]
        public WithKey Key()
        {
            return JsonConvert.DeserializeObject<WithKey>(_json);
        }
    }

    public class WithByteArrayNoConverter
    {
        public byte[] Issuer { get; set; }
    }

    public class WithByteArray
    {
        [JsonCustomSerializer(typeof(ByteArrayCustomSerializer))]
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] Issuer { get; set; }
    }

    public class WithMemory
    {
        [JsonConverter(typeof(MemoryByteJsonConverter))]
        public Memory<byte> Issuer { get; set; }
    }

    public class WithKey
    {
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
}
