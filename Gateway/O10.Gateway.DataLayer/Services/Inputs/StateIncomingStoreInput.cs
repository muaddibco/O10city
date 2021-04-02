using O10.Core.Identity;

namespace O10.Gateway.DataLayer.Services.Inputs
{
    public class StateIncomingStoreInput : IncomingStoreInput
    {
        public long BlockHeight { get; set; }
        public IKey? Source { get; set; }
        public IKey? OriginatingCommitment { get; set; }
    }
}
