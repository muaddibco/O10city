namespace O10.Client.Common.Interfaces.Inputs
{
	public class ProofsChallenge : ProofsRequest
	{
		public string Key { get; set; }
		public string SessionKey { get; set; }
		public string PublicSpendKey { get; set; }
		public string PublicViewKey { get; set; }
	}
}
