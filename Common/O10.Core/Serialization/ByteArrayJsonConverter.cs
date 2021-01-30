using Newtonsoft.Json;
using System;
using O10.Core.ExtensionMethods;
using Newtonsoft.Json.Linq;

namespace O10.Core.Serialization
{
	public class ByteArrayJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.Name == "Byte[]";
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var json = JToken.ReadFrom(reader);
			string value = json.Value<string>();

			if (!string.IsNullOrEmpty(value))
			{
				return value.HexStringToByteArray();
			}

			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			byte[] val = value as byte[];

			writer.WriteValue(val?.ToHexString());
		}
	}
}
