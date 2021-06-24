using O10.Core.Identity;
using System;

namespace O10.Gateway.DataLayer.Services.Inputs
{
    public class IncomingStoreInput
    {
        public long CombinedRegistryBlockHeight { get; set; }
        public long WitnessId { get; set; }
        public ushort TransactionType { get; set; }
        public IKey? Commitment { get; set; }
        public IKey? Destination { get; set; }
        public string Content { get; set; }
        public IKey? TransactionKey { get; set; }
        public IKey? Hash { get; set; }
    }
}
