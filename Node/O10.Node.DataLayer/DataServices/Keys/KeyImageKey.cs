namespace O10.Node.DataLayer.DataServices.Keys
{
	public class KeyImageKey : IDataKey
	{
		public KeyImageKey(byte[] keyImage)
		{
			KeyImage = keyImage;
		}

		public byte[] KeyImage { get; }
	}
}
