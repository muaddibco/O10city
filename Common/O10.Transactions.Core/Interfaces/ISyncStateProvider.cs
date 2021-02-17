using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Transactions.Core.DTOs;

namespace O10.Transactions.Core.Interfaces
{
    [ServiceContract]
    public interface ISyncStateProvider
    {
        Task<SyncBlockModel> GetLastSyncBlock();

        Task<RegistryCombinedBlockModel> GetLastRegistryCombinedBlock();

        Task<IEnumerable<PacketInfo>> GetPacketInfos(IEnumerable<long> witnessIds);

        Task<StatePacketInfo> GetLastPacketInfo(IKey accountPublicKey);
        Task<StatePacketInfo> GetLastPacketInfo(string accountPublicKey);

    }
}
