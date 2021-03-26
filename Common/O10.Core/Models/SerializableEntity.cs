using Newtonsoft.Json;
using O10.Core.Serialization;
using System;
using System.Text;

namespace O10.Core.Models
{
    public abstract class SerializableEntity<T>: ISerializableEntity<T> where T: ISerializableEntity<T>
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, new ByteArrayJsonConverter(), new KeyJsonConverter(), new MemoryByteJsonConverter());
        }

        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(ToString());
        }

        public static T Create(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException($"'{nameof(content)}' cannot be null or empty", nameof(content));
            }

            return JsonConvert.DeserializeObject<T>(content, new ByteArrayJsonConverter(), new KeyJsonConverter(), new MemoryByteJsonConverter());
        }
    }
}
