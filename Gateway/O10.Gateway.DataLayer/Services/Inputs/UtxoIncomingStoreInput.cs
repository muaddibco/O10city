namespace O10.Gateway.DataLayer.Services.Inputs
{
    public class UtxoIncomingStoreInput : IncomingStoreInput
    {
        public byte[] KeyImage { get; set; }

		public byte[] DestinationKey2 { get; set; }
	}
}
