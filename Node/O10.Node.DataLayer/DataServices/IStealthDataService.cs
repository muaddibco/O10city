using O10.Node.DataLayer.DataServices.Keys;

namespace O10.Node.DataLayer.DataServices
{
    public interface IStealthDataService : IChainDataService
    {
        string GetPacketHash(IDataKey dataKey);
    }
}
