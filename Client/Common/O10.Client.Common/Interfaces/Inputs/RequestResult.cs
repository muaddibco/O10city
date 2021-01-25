using System;

namespace O10.Client.Common.Interfaces.Inputs
{
    public class RequestResult
    {
        public bool Result { get; set; }
        public Memory<byte> NewBlindingFactor { get; set; }
        public Memory<byte> NewCommitment { get; set; }
        public Memory<byte> NewTransactionKey { get; set; }
        public Memory<byte> NewDestinationKey { get; set; }
    }
}
