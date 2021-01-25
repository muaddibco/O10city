using Newtonsoft.Json;
using System;
using O10.Core.ExtensionMethods;

namespace O10.Core.Logging
{
	public class ByteArrayJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.Name == "Byte[]";
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			string val = existingValue?.ToString();

			if(!string.IsNullOrEmpty(val))
			{
				return val.HexStringToByteArray();
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
