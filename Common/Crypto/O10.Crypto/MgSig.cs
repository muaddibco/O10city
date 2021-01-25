namespace O10.Crypto
{
	public class MgSig
	{
		private byte[][][] _ss;
		private byte[] _cc;
		private byte[][] _ii;

		public MgSig()
		{
			CC = new byte[32];
		}

		public byte[][][] SS { get => _ss; set => _ss = value; }
		public byte[] CC { get => _cc; set => _cc = value; }

		/// <summary>
		/// this field is not serialized because it can be reconstructed 
		/// </summary>
		internal byte[][] II { get => _ii; set => _ii = value; }
	};
}
