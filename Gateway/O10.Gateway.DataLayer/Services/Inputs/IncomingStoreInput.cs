namespace O10.Gateway.DataLayer.Services.Inputs
{
    public class IncomingStoreInput : StoreInput
    {
        public ulong CombinedRegistryBlockHeight { get; set; }
        public long WitnessId { get; set; }
    }
}
