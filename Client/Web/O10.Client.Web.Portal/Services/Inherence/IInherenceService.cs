using System.Threading;
using System.Threading.Tasks;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;

namespace O10.Client.Web.Portal.Services.Inherence
{
    [ExtensionPoint]
    public interface IInherenceService : IUpdater
    {
        string Name { get; }
        string Alias { get; }
        string Description { get; }
        string Target { get; }
        long AccountId { get; }
        void Initialize(CancellationToken cancellationToken);
        TaskCompletionSource<InherenceData> GetIdentityProofsAwaiter(string sessionKey);

        void RemoveIdentityProofsAwaiter(string sessionKey);
    }
}
