namespace O10.Gateway.DataLayer.Services.Inputs
{
    public class StateIncomingStoreInput : IncomingStoreInput
    {
        public ulong BlockHeight { get; set; }
        public byte[] Source { get; set; }
    }
}
