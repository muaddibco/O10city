namespace O10.Node.Network.Topology
{
    public enum NodeRole : byte
    {
        TransactionsRegistrationLayer,
        StorageLayer,
        DeferredConsensusLayer,
        SynchronizationLayer
    }
}
