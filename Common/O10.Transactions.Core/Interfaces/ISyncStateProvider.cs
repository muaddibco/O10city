using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Crypto.Models;
using O10.Transactions.Core.DTOs;

namespace O10.Transactions.Core.Interfaces
{
    [ServiceContract]
    public interface ISyncStateProvider
    {
        Task<SyncInfoDTO> GetLastSyncBlock();

        Task<AggregatedRegistrationsTransactionDTO> GetLastRegistryCombinedBlock();

        Task<IEnumerable<TransactionBase>> GetTransactions(IEnumerable<long> witnessIds);

        Task<StatePacketInfo> GetLastPacketInfo(IKey accountPublicKey);
        Task<StatePacketInfo> GetLastPacketInfo(string accountPublicKey);

    }
}
