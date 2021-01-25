using O10.Core.Architecture;
using O10.Core.Predicates;

namespace O10.Node.Core.Registry
{
    [RegisterExtension(typeof(IPredicate), Lifetime = LifetimeManagement.Singleton)]
    public class IsBlockProducerPredicate : IPredicate
    {
        public string Name => "IsBlockProducer";

        public bool Evaluate(params object[] args)
        {
            //TODO: replace with getting from configuration
            return true;
        }
    }
}
