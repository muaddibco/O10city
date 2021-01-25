namespace O10.Network.Topology
{
    public enum NodeRole : byte
    {
        TransactionsRegistrationLayer,
        StorageLayer,
        DeferredConsensusLayer,
        SynchronizationLayer
    }
}
