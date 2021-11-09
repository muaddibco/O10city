using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Core.Translators;
using O10.Core.ExtensionMethods;
using O10.Node.Core.DataLayer.DataContexts;
using O10.Node.Network.Topology;

namespace O10.Node.Core.DataLayer
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class NodesMapper : TranslatorBase<NodeRecord, NodeEntity>
    {
        private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;

        public NodesMapper(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _identityKeyProvidersRegistry = identityKeyProvidersRegistry;
        }

        public override NodeEntity Translate(NodeRecord nodeDataModel)
        {
            if (nodeDataModel is null)
            {
                throw new System.ArgumentNullException(nameof(nodeDataModel));
            }

            return new NodeEntity
            {
                Key = _identityKeyProvidersRegistry.GetInstance().GetKey(nodeDataModel.PublicKey.HexStringToByteArray()),
                IPAddress = System.Net.IPAddress.Parse(nodeDataModel.IPAddress)
            };
        }
    }
}
