namespace O10.Core.Cryptography
{
    public class BorromeanRingSignatureEx
	{
		public BorromeanRingSignatureEx()
		{

		}
		public byte[] E { get; set; }
		public byte[][][] S { get; set; } = new byte[][][] { new byte[64][], new byte[64][] };
    }
}
