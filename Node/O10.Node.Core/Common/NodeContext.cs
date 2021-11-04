using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Core.States;

namespace O10.Node.Core.Common
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class NodeContext : INodeContext
    {
        public const string NAME = nameof(INodeContext);

        public string Name => NAME;

        public IKey AccountKey { get; private set; }

        public ISigningService SigningService { get; private set; }

        public void Initialize(ISigningService signingService)
        {
            SigningService = signingService;
            AccountKey = SigningService.PublicKeys[0];
        }
    }
}
