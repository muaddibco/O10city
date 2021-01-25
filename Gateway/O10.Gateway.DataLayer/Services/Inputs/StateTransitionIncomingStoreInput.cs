namespace O10.Gateway.DataLayer.Services.Inputs
{
    public class StateTransitionIncomingStoreInput : StateIncomingStoreInput
    {
        public byte[] TransactionKey { get; set; }
    }
}
