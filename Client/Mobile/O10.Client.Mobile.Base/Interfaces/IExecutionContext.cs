using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ServiceContract]
    public interface IExecutionContext
    {
        long AccountId { get; }

        bool IsInitialized { get; }

        IGatewayService GatewayService { get; }

        IBoundedAssetsService RelationsBindingService { get; }

        IStealthTransactionsService TransactionsService { get; }

        IStealthClientCryptoService ClientCryptoService { get; }

        void InitializeUtxoExecutionServices(long accountId, byte[] secretSpendKey, byte[] secretViewKey);

        void UnregisterExecutionServices();

        Task<IssuerActionDetails> GetActionDetails(string uri);

        ISourceBlock<bool> InitializationCompleted { get; }

        TaskCompletionSource<byte[]> GenerateBindingKey(string key, string pwd);

        TaskCompletionSource<byte[]> GetIssuerBindingKeySource(string key);

        TaskCompletionSource<byte[]> GetBindingKeySource(string pwd);

        bool IsBindingKeyValid(string key);

        Task<TaskCompletionSource<byte[]>> GetBindingKeySourceWithBio(string key);

        string LastExpandedKey { get; set; }

        IPropagatorBlock<string, string> NavigationPipe { get; }
    }
}
