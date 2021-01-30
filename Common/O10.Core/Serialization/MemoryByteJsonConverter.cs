using Newtonsoft.Json;
using System;
using O10.Core.ExtensionMethods;
using Newtonsoft.Json.Linq;

namespace O10.Core.Serialization
{
	public class MemoryByteJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.Name == "Memory<Byte>";
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var json = JToken.ReadFrom(reader);
			string value = json.Value<string>();

			if (!string.IsNullOrEmpty(value))
			{
				return (Memory<byte>)value.HexStringToByteArray();
			}

			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var val = (Memory<byte>)value;

			writer.WriteValue(val.ToHexString());
		}
	}
}
