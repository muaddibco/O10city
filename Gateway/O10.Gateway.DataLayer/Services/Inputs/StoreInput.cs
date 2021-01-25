namespace O10.Gateway.DataLayer.Services.Inputs
{
    public class StoreInput
    {
        public ulong SyncBlockHeight { get; set; }
        public ushort BlockType { get; set; }
        public byte[] Commitment { get; set; }
        public byte[] Destination { get; set; }
        public byte[] Content { get; set; }
    }
}
