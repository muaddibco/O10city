using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Core.Cryptography
{
    public class BorromeanRingSignatureEx
	{
		public BorromeanRingSignatureEx()
		{

		}

		[JsonConverter(typeof(ByteArrayJsonConverter))]
		public byte[] E { get; set; }

		public byte[][][] S { get; set; } = new byte[][][] { new byte[64][], new byte[64][] };
    }
}
