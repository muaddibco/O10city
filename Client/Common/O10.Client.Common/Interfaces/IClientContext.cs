using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IClientContext
    {
        long AccountId { get; }
        void Initialize(long accountId);
    }
}
