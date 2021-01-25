using O10.Core.Communication;

namespace O10.Client.Common.Communication
{
    public class IntermediateOutput
    {
        public byte[] NewBlindingFactor { get; set; }
        public int Pos { get; set; }
        public IPacketProvider PacketProvider { get; set; }
    }
}
