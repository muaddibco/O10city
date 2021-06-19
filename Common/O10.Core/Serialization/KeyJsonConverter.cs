#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;

namespace O10.Core.Serialization
{
    public class KeyJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IKey).IsAssignableFrom(objectType);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var json = JToken.ReadFrom(reader);
            string value = json.Value<string>();

            if(string.IsNullOrEmpty(value))
            {
                return null;
            }

            return value.Length switch
            {
                64 => new Key32(value.HexStringToByteArray()),
                32 => new Key16(value.HexStringToByteArray()),
                _ => throw new ArgumentException("Not supported length of Key"),
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => serializer?.Serialize(writer, value?.ToString());

        private IKey? DeserializeLocationRuntime(JToken json, Type locationType)
        {
            MethodInfo? mi = typeof(JToken)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "ToObject" && m.GetParameters().Length == 0 && m.IsGenericMethod)
                .FirstOrDefault()
                ?.MakeGenericMethod(locationType);
            var key = mi?.Invoke(json, null);
            return key as IKey;
        }
    }
}
