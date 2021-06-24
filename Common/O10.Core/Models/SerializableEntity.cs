using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using O10.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace O10.Core.Models
{
    public abstract class SerializableEntity: ISerializableEntity
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, new ByteArrayJsonConverter(), new KeyJsonConverter(), new MemoryByteJsonConverter());
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this,
                new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new ByteArrayJsonConverter(), new KeyJsonConverter(), new MemoryByteJsonConverter() },
                    TypeNameHandling = TypeNameHandling.All
                });
        }

        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(ToString());
        }

        public static T? Create<T>(string content) where T : class, ISerializableEntity
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException($"'{nameof(content)}' cannot be null or empty", nameof(content));
            }

            return JsonConvert.DeserializeObject<T>(content, 
                new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new ByteArrayJsonConverter(), new KeyJsonConverter(), new MemoryByteJsonConverter() },
                    TypeNameHandling = TypeNameHandling.All
                });
        }
    }
}
