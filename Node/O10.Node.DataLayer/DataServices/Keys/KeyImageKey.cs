namespace O10.Node.DataLayer.DataServices.Keys
{
	public class KeyImageKey : IDataKey
	{
		public KeyImageKey(string keyImage)
		{
			KeyImage = keyImage;
		}

		public string KeyImage { get; }
	}
}
