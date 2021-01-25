using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Core.Translators;
using O10.Core.ExtensionMethods;
using O10.Node.Core.DataLayer.DataContexts;

namespace O10.Node.Core.DataLayer
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class NodesMapper : TranslatorBase<NodeRecord, Network.Topology.Node>
    {
        private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;

        public NodesMapper(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _identityKeyProvidersRegistry = identityKeyProvidersRegistry;
        }

        public override Network.Topology.Node Translate(NodeRecord nodeDataModel)
        {
            if (nodeDataModel is null)
            {
                throw new System.ArgumentNullException(nameof(nodeDataModel));
            }

            return new Network.Topology.Node
            {
                Key = _identityKeyProvidersRegistry.GetInstance().GetKey(nodeDataModel.PublicKey.HexStringToByteArray()),
                IPAddress = System.Net.IPAddress.Parse(nodeDataModel.IPAddress)
            };
        }
    }
}
