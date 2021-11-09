using O10.Core.Identity;
using O10.Core.States;
using O10.Crypto.Models;

namespace O10.Node.Core.Common
{
    public interface INodeContext : IState
    {
        IKey AccountKey { get; }

        void Initialize(ISigningService signingService);

        ISigningService SigningService { get; }
    }
}
